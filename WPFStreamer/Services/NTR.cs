using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using UWPStreamer.Helpers;
using Windows.Graphics.Imaging;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;

namespace UWPStreamer.Services
{
    public class NTR : INotifyPropertyChanged
    {
        CancellationTokenSource tokenSource;
        CancellationToken ct;

        Task ntrRemotePlayTask;

        UdpClient client;
        List<byte> priorityScreenBuffer = new List<byte>();
        List<byte> secondaryScreenBuffer = new List<byte>();

        private byte priorityExpectedFrame = 0;
        private byte secondaryExpectedFrame = 0;

        private byte priorityExpectedPacket = 0;
        private byte secondaryExpectedPacket = 0;

        private int activePriorityMode = 1;

        public event PropertyChangedEventHandler PropertyChanged;



        //bottom
        private ImageSource screen0;

        public ImageSource Screen0
        {
            get { return screen0; }
            set
            {
                screen0 = value;
                OnPropertyChanged();
            }
        }

        //top
        private ImageSource screen1;


        public ImageSource Screen1
        {
            get { return screen1; }
            set
            {
                screen1 = value;
                OnPropertyChanged();
            }
        }

        public NTR()
        {
            //Screen1 = new BitmapImage(new Uri("ms-appx:///Assets/placeholder.png"));
        }

        //This is the TCP package that needs to be sent to the N3DS.Under other cirumstances we'd want to change other bytes but to
        //initialize remoteplay we only need to care about bytes 0x10, 0x11, 0x14 and 0x1A.

        //Bytes 0x10 and 0x14 contain, respectively, the priority factor and JPEG quality variables.All we need to do is to convert
        //them from DEC to HEX et voilà, they work.


        //Byte 0x11 is the priority mode byte. This is a weird one: internally, 1 is for top screen and 0 is for bottom screen.If
        //you've used NTRClient, howerer, you've probably noticed that the boolean is actually FLIPPED, so 0 is top screen and
        //1 is bottom screen.I don't know why cell9 thought this was a good idea so, as we're sending a RAW package here, I've
        //decided to NOT flip the boolean.This way, there won't be any confusion regarding what this value actually means, and

        //1 will always mean top screen and 0 bottom screen in this source code.
        //Finally, byte 0x1A contains the QoS value.I have no idea why, but NTR expects it to be double its intended value.

        public async Task<bool> InitRemoteplay(string ip, int priorityMode = 1, int priorityFactor = 1, int quality = 75, int qosValue = 15)
        {
            activePriorityMode = priorityMode;

            string serverHost = ip;
            int serverPort = 8000;

            var hexString = "78563412B80B00000000000085030000";
            hexString = hexString + priorityFactor.ToString("X2");
            hexString = hexString + priorityMode.ToString("X2") + "0000";
            hexString = hexString + quality.ToString("X2") + "0000000000";
            hexString = hexString + (qosValue * 2).ToString("X2");
            string zeroPad = new string('0', 114);
            hexString = hexString + zeroPad;

            TcpClient socket = new TcpClient();
            await socket.ConnectAsync(serverHost, serverPort);

            BinaryWriter writer = new BinaryWriter(socket.GetStream());
            writer.Write(hexString.StringToByteArray());

            writer.Flush();

            socket.Close();
            Task.Delay(2000).Wait();

            socket = new TcpClient();
            await socket.ConnectAsync(serverHost, serverPort);
            socket.Close();

            return true;
        }

        public void NTRRemoteplayConnect(string ip)
        {

            var intAddress = (long)(uint)BitConverter.ToInt32(IPAddress.Parse(ip).GetAddressBytes(), 0);

            if (client != null)
                disconnect();

            client = new UdpClient(8001);
            IPEndPoint server = new IPEndPoint(intAddress, 8001);

            tokenSource = new CancellationTokenSource();
            ct = tokenSource.Token;

            ntrRemotePlayTask = new Task(() => { while (true) NTRRemoteplayReadJPEG(client.Receive(ref server)); }, ct);
            ntrRemotePlayTask.Start();

        }

        private async void NTRRemoteplayReadJPEG(byte[] data)
        {

            //A remoteplay packet sent by NTR looks like this

            //== HEADER ==
            //0x00: Frame ID
            //0x01: First Nibble:if set to 1, it means that the packet is the last one in a JPEG stream.Second Nibble:Screen, 1 = Top / 0 = Bottom
            //0x02: Image format, usually this is set to 2
            //0x04: Packet number in JPEG stream

            var bytes = data.ToList();

            if (bytes.Count < 4)
                return;

            byte currentFrame = bytes[0];
            byte currentScreen = (byte)(bytes[1] & 0x0F);
            byte isLastPacket = (byte)((bytes[1] & 0xF0) >> 4);
            int currentPacket = bytes[3];

            //init to currentFrame 
            if (priorityExpectedFrame == 0 && currentScreen == activePriorityMode)
            {
                priorityExpectedFrame = currentFrame;
            }
            else if (secondaryExpectedFrame == 0)
            {
                secondaryExpectedFrame = currentFrame;
            }


            //= BODY ==
            //0x05 to 0x0n: JPEG data
            if (priorityExpectedFrame == currentFrame && priorityExpectedPacket == currentPacket &&  activePriorityMode == currentScreen)
            {
                //priority screen
                priorityScreenBuffer.AddRange(bytes.GetRange(4, bytes.Count - 4));
                priorityExpectedPacket++;

                if(isLastPacket == 1)
                {
                    await TryDisplayImage(priorityScreenBuffer, currentScreen);
                    priorityExpectedFrame = 0;
                    priorityExpectedPacket = 0;
                }
            }
            else if (currentScreen == activePriorityMode)
            {
                //Priority Packet Dropped (unexpected packet or frame)
                priorityScreenBuffer.Clear();
                priorityExpectedFrame = 0;
                priorityExpectedPacket = 0;

                return;
            }
            else if(secondaryExpectedPacket == currentPacket)
            {
                //secondary screen
                secondaryScreenBuffer.AddRange(bytes.GetRange(4, bytes.Count - 4));
                secondaryExpectedPacket++;

                if(isLastPacket == 1)
                {
                    await TryDisplayImage(secondaryScreenBuffer, currentScreen);
                    secondaryExpectedFrame = 0;
                    secondaryExpectedPacket = 0;
                }

                return;
            }
            else
            {
                //Secondary Packet Dropped (unexpected packet or frame)
                secondaryScreenBuffer.Clear();
                secondaryExpectedFrame = 0;
                secondaryExpectedPacket = 0;
            }


        }

        private async Task TryDisplayImage(List<byte> screenBuffer, int screen)
        {
                await DispatcherHelper.RunAsync(
                () =>
                    {
                        try
                        {
                            var stream = new MemoryStream(screenBuffer.ToArray());
                            stream.Seek(0, SeekOrigin.Begin);

                            var bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            bitmapImage.StreamSource = stream;
                            bitmapImage.EndInit();

                            if (screen == 1)
                                Screen1 = bitmapImage;
                            else
                                Screen0 = bitmapImage;
                        }
                        catch (Exception e)
                        {
                            //something happened
                        }
                    }
                );

                screenBuffer.Clear();                
        }

        internal void disconnect()
        {
            if (ct.CanBeCanceled)
                tokenSource.Cancel();

            if (client != null)
                client.Close();
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            //C# 6 null-safe operator. No need to check for event listeners
            //If there are no listeners, this will be a noop
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}

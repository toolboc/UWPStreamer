using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using UWPStreamer.Helpers;
using Windows.Graphics.Imaging;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace UWPStreamer.Services
{
    public class NTR : INotifyPropertyChanged
    {
        DatagramSocket socket = new DatagramSocket();
        List<byte> priorityScreenBuffer = new List<byte>();
        List<byte> secondaryScreenBuffer = new List<byte>();

        private byte priorityExpectedFrame = 0;
        private byte secondaryExpectedFrame = 0;

        private byte priorityExpectedPacket = 0;
        private byte secondaryExpectedPacket = 0;

        private int activePriorityMode = 1;

        public event PropertyChangedEventHandler PropertyChanged;

        //top
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

        //bottom
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


        public async Task<bool> InitRemoteplay(string ip, int priorityMode = 1, int priorityFactor = 5, int quality = 75, int qosValue = 15)
        {
            activePriorityMode = priorityMode;

            HostName serverHost = new HostName(ip);
            string serverPort = "8000";

            var hexString = "78563412B80B00000000000085030000";
            hexString = hexString + priorityFactor.ToString("X2");
            hexString = hexString + priorityMode.ToString("X2") + "0000";
            hexString = hexString + quality.ToString("X2") + "0000000000";
            hexString = hexString + (qosValue * 2).ToString("X2");
            string zeroPad = new string('0', 114);
            hexString = hexString + zeroPad;

            StreamSocket socket = new StreamSocket();
            await socket.ConnectAsync(serverHost, serverPort);

            BinaryWriter writer = new BinaryWriter(socket.OutputStream.AsStreamForWrite());
            writer.Write(hexString.StringToByteArray());

            writer.Flush();

            socket.Dispose();
            Task.Delay(3000).Wait();

            socket = new StreamSocket();
            await socket.ConnectAsync(serverHost, serverPort);
            socket.Dispose();

            return true;
        }

        public async void NTRRemoteplayConnect()
        {
            string serverPort = "8001";
            socket.MessageReceived += NTRRemoteplayReadJPEG;
            await socket.BindServiceNameAsync(serverPort);
        }

        private async void NTRRemoteplayReadJPEG(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {

            Stream streamIn = args.GetDataStream().AsStreamForRead();
            BinaryReader reader = new BinaryReader(streamIn);

            //A remoteplay packet sent by NTR looks like this

            //== HEADER ==
            //0x00: Frame ID
            //0x01: First Nibble:if set to 1, it means that the packet is the last one in a JPEG stream.Second Nibble:Screen, 1 = Top / 0 = Bottom
            //0x02: Image format, usually this is set to 2
            //0x04: Packet number in JPEG stream

            var bytes = reader.ReadBytes(1448).ToList();
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
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                    {
                        var bitmapImage = new BitmapImage();

                        var stream = new InMemoryRandomAccessStream();
                        await stream.WriteAsync(screenBuffer.ToArray().AsBuffer());
                                        stream.Seek(0);

                                        bitmapImage.SetSource(stream);

                                        if(screen == 1)
                                            Screen1 = bitmapImage;
                                        else
                                            Screen0 = bitmapImage;
                                    }
                                    );

                screenBuffer.Clear();                
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            //C# 6 null-safe operator. No need to check for event listeners
            //If there are no listeners, this will be a noop
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}

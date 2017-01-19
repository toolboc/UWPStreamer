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
        List<byte> screenBuffer = new List<byte>();

        private byte expectedFrame = 0;
        private byte expectedPacket = 0;

        private int activePriorityMode = 1;

        public event PropertyChangedEventHandler PropertyChanged;

        private ImageSource image;

        public ImageSource Image
        {
            get { return image; }
            set
            {
                image = value;
                OnPropertyChanged();
            }
        }


        public async Task<bool> InitRemoteplay(string ip, int priorityMode = 1, int priorityFactor = 5, int quality = 90, int qosValue = 20)
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
            int currentFrame = bytes[0];
            byte currentScreen = (byte)(bytes[1] & 0x0F);
            byte isLastPacket = (byte)((bytes[1] & 0xF0) >> 4);
            int currentPacket = bytes[3];

            //init currentFrame 
            if (expectedFrame == 0 && bytes[1] == activePriorityMode)
            {
                expectedFrame = bytes[0];
            }


            //= BODY ==
            //0x05 to 0x0n: JPEG data
            if (expectedFrame == currentFrame && expectedPacket == currentPacket &&  activePriorityMode == currentScreen)
            {
                screenBuffer.AddRange(bytes.GetRange(4, bytes.Count - 4));
                expectedPacket++;
            }
            else if (currentScreen == activePriorityMode)
            {
                //Packet Dropped (unexpected packet or frame)
                screenBuffer.Clear();
                expectedFrame = 0;
                expectedPacket = 0;

                return;
            }
            else
            {
                //Packet Dropped (wrong screen)
                return;
            }

            //JPEG Stream ends with "FFD9" | FF = 255, D9 = 217
            if (screenBuffer.Count > 1 && screenBuffer.Last() == 217 && screenBuffer[screenBuffer.Count - 2] == 255)
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                    {
                        var bitmapImage = new BitmapImage();

                        var stream = new InMemoryRandomAccessStream();
                        await stream.WriteAsync(screenBuffer.ToArray().AsBuffer());
                        stream.Seek(0);

                        bitmapImage.SetSource(stream);
                        Image = bitmapImage;
                    }
                    );            

                screenBuffer.Clear();
                expectedFrame = 0;
                expectedPacket = 0;          
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            //C# 6 null-safe operator. No need to check for event listeners
            //If there are no listeners, this will be a noop
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}

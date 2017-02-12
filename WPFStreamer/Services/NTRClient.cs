using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WPFStreamer;

namespace UWPStreamer.Services
{
        public class NTRClient
        {
            public String host;
            public int port;
            public TcpClient tcp;
            public NetworkStream netStream;
            public Task packetRecvThread;
            private object syncLock = new object();
            int heartbeatSendable;
            int timeout;
            public delegate void logHandler(string msg);
            UInt32 currentSeq;
            public volatile int progress = -1;

            int readNetworkStream(NetworkStream stream, byte[] buf, int length)
            {
                int index = 0;
                bool useProgress = false;

                if (length > 100000)
                {
                    useProgress = true;
                }
                do
                {
                    if (useProgress)
                    {
                        progress = (int)(((double)(index) / length) * 100);
                    }
                    int len = stream.Read(buf, index, length - index);
                    if (len == 0)
                    {
                        return 0;
                    }
                    index += len;
                } while (index < length);
                progress = -1;
                return length;
            }

            void packetRecvThreadStart()
            {
                byte[] buf = new byte[84];
                UInt32[] args = new UInt32[16];
                int ret;
                NetworkStream stream = netStream;

                while (true)
                {
                    try
                    {
                        ret = readNetworkStream(stream, buf, buf.Length);
                        if (ret == 0)
                        {
                            break;
                        }
                        int t = 0;
                        UInt32 magic = BitConverter.ToUInt32(buf, t);
                        t += 4;
                        UInt32 seq = BitConverter.ToUInt32(buf, t);
                        t += 4;
                        UInt32 type = BitConverter.ToUInt32(buf, t);
                        t += 4;
                        UInt32 cmd = BitConverter.ToUInt32(buf, t);
                        for (int i = 0; i < args.Length; i++)
                        {
                            t += 4;
                            args[i] = BitConverter.ToUInt32(buf, t);
                        }
                        t += 4;
                        UInt32 dataLen = BitConverter.ToUInt32(buf, t);

                        if (magic != 0x12345678)
                        {
                            break;
                        }

                        if (cmd == 0)
                        {
                            if (dataLen != 0)
                            {
                                byte[] dataBuf = new byte[dataLen];
                                readNetworkStream(stream, dataBuf, dataBuf.Length);
                            }
                            lock (syncLock)
                            {
                                heartbeatSendable = 1;
                            }
                            continue;
                        }
                        if (dataLen != 0)
                        {
                            byte[] dataBuf = new byte[dataLen];
                            readNetworkStream(stream, dataBuf, dataBuf.Length);
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
                disconnect(false);
            }

            public void setServer(String serverHost, int serverPort)
            {
                host = serverHost;
                port = serverPort;
            }

            public Boolean connectToServer()
            {
                if (tcp != null)
                {
                    disconnect();
                }
                tcp = new TcpClient();
                tcp.NoDelay = true;
                try
                {
                    if (tcp.ConnectAsync(host, port).Wait(1000))
                    {
                        currentSeq = 0;
                        netStream = tcp.GetStream();
                        heartbeatSendable = 1;
                        packetRecvThread = new Task(packetRecvThreadStart);
                        packetRecvThread.Start();
                        App.Connected = true;
                    }
                    else
                    {
                        App.Connected = false;
                    }
                }
                catch
                {
                    App.Connected = false;
                }

                return App.Connected;
            }

            public void disconnect(bool waitPacketThread = true)
            {
                try
                {
                    if (tcp != null)
                    {
                        tcp.Close();
                    }
                    if (waitPacketThread)
                    {
                        if (packetRecvThread != null)
                        {
                            packetRecvThread.Wait();
                        }
                    }
                }
                catch { }
                tcp = null;
                App.Connected = false;
            }

            public void sendPacket(UInt32 type, UInt32 cmd, UInt32[] args, UInt32 dataLen)
            {
                int t = 0;
                currentSeq += 1000;
                byte[] buf = new byte[84];
                BitConverter.GetBytes(0x12345678).CopyTo(buf, t);
                t += 4;
                BitConverter.GetBytes(currentSeq).CopyTo(buf, t);
                t += 4;
                BitConverter.GetBytes(type).CopyTo(buf, t);
                t += 4;
                BitConverter.GetBytes(cmd).CopyTo(buf, t);
                for (int i = 0; i < 16; i++)
                {
                    t += 4;
                    UInt32 arg = 0;
                    if (args != null)
                    {
                        arg = args[i];
                    }
                    BitConverter.GetBytes(arg).CopyTo(buf, t);
                }
                t += 4;
                BitConverter.GetBytes(dataLen).CopyTo(buf, t);
                try
                {
                    netStream.Write(buf, 0, buf.Length);
                }
                catch (Exception)
                {
                }
            }

            public void sendWriteMemPacket(UInt32 addr, UInt32 pid, byte[] buf)
            {
                UInt32[] args = new UInt32[16];
                args[0] = pid;
                args[1] = addr;
                args[2] = (UInt32)buf.Length;
                sendPacket(1, 10, args, args[2]);
                netStream.Write(buf, 0, buf.Length);
            }

            public void sendHeartbeatPacket()
            {
                if (App.Connected)
                {
                    lock (syncLock)
                    {
                        if (heartbeatSendable == 1)
                        {
                            heartbeatSendable = 0;
                            sendPacket(0, 0, null, 0);
                        }
                        else
                        {
                            timeout++;
                            if (timeout == 5)
                            {
                                disconnect(false);
                            }
                        }
                    }
                }

            }
        }
    }

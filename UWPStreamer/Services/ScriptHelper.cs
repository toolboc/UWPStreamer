using System;
using UWPStreamer;

namespace InputRedirectionNTR
{
    public class ScriptHelper
    {
        public Boolean connect(string host, int port)
        {
            App.ntrClient.setServer(host, port);
            return App.ntrClient.connectToServer();
        }

        public void disconnect()
        {
            App.ntrClient.disconnect();
        }

        public void write(uint addr, byte[] buf, int pid = -1)
        {
            App.ntrClient.sendWriteMemPacket(addr, (uint)pid, buf);
        }
    }
}
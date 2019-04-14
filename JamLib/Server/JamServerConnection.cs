using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JamLib.Server
{
    public class JamServerConnection: IDisposable
    {
        private readonly SslStream stream;

        private bool alive;

        public JamServerConnection(SslStream stream)
        {
            this.stream = stream;
            alive = true;

            Task.Run(() => Listen());
        }
        
        public void Dispose()
        {
            alive = false;
            stream.Close();
        }

        public int Send(JamPacket packet)
        {
            int sentBytes = 0;

            Task.Run(() => { sentBytes = packet.Send(stream); });

            while (sentBytes == 0)
                Thread.Sleep(50);

            return sentBytes;
        }

        public void Listen()
        {
            while (alive)
            {
                JamPacket packet = JamPacket.Receive(stream);
                Console.WriteLine(packet);
            }
        }
    }
}

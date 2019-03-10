using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JamLib.Server
{
    public class JamServerConnection: IDisposable
    {
        private Socket socket;

        private bool alive;

        public JamServerConnection(Socket socket)
        {
            this.socket = socket;
            alive = true;

            Thread listeningThread = new Thread(Listen);
            listeningThread.Start();
        }
        
        public void Dispose()
        {
            alive = false;

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        public int Send(JamPacket packet)
        {
            int sentBytes = 0;

            Thread thread = new Thread(() =>
            {
                sentBytes = packet.Send(socket);
            });
            thread.Start();

            while (sentBytes == 0)
                Thread.Sleep(50);

            return sentBytes;
        }

        public void Listen()
        {
            while (alive)
            {
                JamPacket packet = JamPacket.Receive(socket);
                Console.WriteLine(packet);
            }
        }
    }
}

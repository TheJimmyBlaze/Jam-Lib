using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JamLib.Client
{
    public class JamClient: IDisposable
    {
        private AutoResetEvent connectionCompleted = new AutoResetEvent(false);

        private Socket socket;
        private bool alive;

        public void Connect(string ip, int port, int timeout)
        {
            IPHostEntry host = Dns.GetHostEntry(ip);
            IPAddress address = host.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(address, port);

            Socket socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            socket.BeginConnect(endPoint, new AsyncCallback(ConnectCallback), socket);
            connectionCompleted.WaitOne(timeout);

            if (socket.Connected)
            {
                alive = true;

                Thread listeningThread = new Thread(Listen);
                listeningThread.Start();
            }
        }

        private void ConnectCallback(IAsyncResult result)
        {
            socket = result.AsyncState as Socket;
            socket.EndConnect(result);
            connectionCompleted.Set();
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JamLib.Server
{
    public class JamServer: IDisposable
    {
        private AutoResetEvent acceptCompleted = new AutoResetEvent(false);

        private bool alive;

        private List<JamServerConnection> connections = new List<JamServerConnection>();

        public void Start(int port)
        {
            alive = true;

            Thread listeningThread = new Thread(() => { Listen(port); });
            listeningThread.Start();
        }

        public void Dispose()
        {
            alive = false;
        }

        public void Listen(int port)
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress address = host.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(address, port);

            Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(endPoint);
            socket.Listen(100);

            while (alive)
            {
                socket.BeginAccept(new AsyncCallback(AcceptCallback), socket);
                acceptCompleted.WaitOne();
            }
        }

        public void AcceptCallback(IAsyncResult result)
        {
            Socket socket = result.AsyncState as Socket;
            Socket connectee = socket.EndAccept(result);

            JamServerConnection connection = new JamServerConnection(connectee);
            connections.Add(connection);

            acceptCompleted.Set();
        }
    }
}

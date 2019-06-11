using JamLib.Database;
using JamLib.Domain;
using JamLib.Domain.Serialization;
using JamLib.Packet;
using JamLib.Packet.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace JamLib.Client
{
    public class JamClient: IDisposable
    {
        #region Event Handlers
        public class MessageReceivedEventArgs: EventArgs
        {
            public JamPacket Packet { get; set; }
        }

        public class LoginResultEventArgs: EventArgs
        {
            public LoginResponse.LoginResult Result { get; set; }
        }

        public EventHandler<MessageReceivedEventArgs> MessageReceivedEvent;
        public EventHandler<LoginResultEventArgs> LoginResultEvent;
        public EventHandler DisposedEvent;

        public void OnMessageReceived(MessageReceivedEventArgs e)
        {
            MessageReceivedEvent?.Invoke(this, e);
        }

        public void OnLoginResult(LoginResultEventArgs e)
        {
            LoginResultEvent?.Invoke(this, e);
        }

        public void OnDisposed(EventArgs e)
        {
            DisposedEvent?.Invoke(this, e);
        }
        #endregion

        private readonly AutoResetEvent connectionCompleted = new AutoResetEvent(false);

        public readonly string AppSigniture;
        private SslStream stream;
        private bool alive;

        private readonly ConcurrentQueue<JamPacket> packetSendQueue = new ConcurrentQueue<JamPacket>();

        public Account Account { get; set; }

        public readonly ISerializer Serializer;

        public bool IsConnected
        {
            get { return stream != null && stream.CanWrite && stream.CanRead; }
        }
        
        public JamClient(string appSigniture, ISerializer serializer)
        {
            AppSigniture = appSigniture;
            Serializer = serializer;
        }

        protected virtual bool ValidateCertificate(object sender, X509Certificate serverCertificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            //This basic method simply returns true, method should be overriden if certificate validation implementation is required.
            return true;
        }

        public void Connect(string ip, int port, int timeout)
        {
            try
            {
                TcpClient client = new TcpClient(ip, port);
                SslStream inProgressStream = new SslStream(client.GetStream(), false, ValidateCertificate);

                inProgressStream.BeginAuthenticateAsClient(ip, null, SslProtocols.Default, false, ConnectCallback, inProgressStream);
                connectionCompleted.WaitOne(timeout);

                if (stream != null && stream.IsAuthenticated)
                {
                    alive = true;

                    Task.Run(() => Listen());
                    Task.Run(() => SendPacketsFromQueue());
                }
            }
            catch (SocketException) { }
        }

        private void ConnectCallback(IAsyncResult result)
        {
            stream = result.AsyncState as SslStream;
            stream.EndAuthenticateAsClient(result);
            connectionCompleted.Set();
        }

        public void Dispose()
        {
            if (alive)
            {
                alive = false;
                stream?.Close();
                OnDisposed(null);
            }
        }

        public void Login(string username, string password)
        {
            LoginRequest loginRequest = new LoginRequest(AppSigniture, username, password, Serializer);

            JamPacket packet = new JamPacket(Guid.Empty, Guid.Empty, LoginRequest.DATA_TYPE, loginRequest.GetBytes());
            Send(packet);
        }

        public void HandleLoginResponse(JamPacket loginResponsePacket)
        {
            if (loginResponsePacket.Header.DataType != LoginResponse.DATA_TYPE)
                return;

            LoginResponse response = new LoginResponse(loginResponsePacket.Data, Serializer);
            LoginResultEventArgs resultArgs = new LoginResultEventArgs() { Result = response.Result };

            if (response.Result == LoginResponse.LoginResult.Good)
            {
                Account = response.Account;
                OnLoginResult(resultArgs);
            }
            else
            {
                OnLoginResult(resultArgs);
                Dispose();
            }
        }

        public void RespondToPing(JamPacket pingPacket)
        {
            if (pingPacket.Header.DataType != PingRequest.DATA_TYPE)
                return;

            PingRequest request = new PingRequest(pingPacket.Data, Serializer);

            PingResponse response = new PingResponse(request.PingTimeUtc, DateTime.UtcNow, Serializer);

            Guid accountID = Guid.Empty;
            if (Account != null)
                accountID = Account.AccountID;

            JamPacket responsePacket = new JamPacket(pingPacket.Header.Sender, accountID, PingResponse.DATA_TYPE, response.GetBytes());
            Send(responsePacket);
        }

        public void Send(JamPacket packet)
        {
            packetSendQueue.Enqueue(packet);
        }

        private void SendPacketsFromQueue()
        {
            while (alive)
            {
                Thread.Sleep(50);

                if (packetSendQueue.Count > 0 && stream.CanWrite)
                {
                    try
                    {
                        if (packetSendQueue.TryDequeue(out JamPacket sendPacket))
                            sendPacket.Send(stream);
                    }
                    catch (IOException)
                    {
                        Dispose();
                    }
                }
            }
        }

        private void Listen()
        {
            while (alive)
            {
                JamPacket packet = JamPacket.Receive(stream);
                if (packet == null)
                {
                    Dispose();
                    return;
                }

                InternalClientInterpreter.Interpret(this, packet);
            }
        }
    }
}

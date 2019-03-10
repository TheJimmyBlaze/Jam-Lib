using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JamLib
{
    public class JamPacket
    {
        private struct ReceiveState
        {
            public Socket Socket { get; set; }
            public JamPacket Packet { get; set; }
            public byte[] HeaderBytes { get; set; }
        }

        private AutoResetEvent sendCompleted = new AutoResetEvent(false);
        private static AutoResetEvent receiveCompleted = new AutoResetEvent(false);
        
        public JamPacketHeader Header { get; set; }
        public byte[] Data { get; set; }

        private int bytesRead = 0;

        public JamPacket() { }

        public JamPacket(Guid recipient, Guid sender, bool encrypted, byte[] data)
        {
            Header = new JamPacketHeader()
            {
                Receipient = recipient,
                Sender = sender,
                SendTimeUTC = DateTime.UtcNow,
                DataEncrypted = encrypted,
                DataLength = data.Length
            };

            Data = data;
        }

        public override string ToString()
        {
            return Encoding.ASCII.GetString(Data);
        }

        public int Send(Socket socket)
        {
            byte[] headerBytes = Header.GetBytes();
            byte[] sendBytes = new byte[headerBytes.Length + Data.Length];

            headerBytes.CopyTo(sendBytes, 0);
            Data.CopyTo(sendBytes, headerBytes.Length);
            
            socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, new AsyncCallback(SendCallback), socket);
            sendCompleted.WaitOne();

            return bytesRead;
        }

        private void SendCallback(IAsyncResult result)
        {
            Socket socket = (Socket)result.AsyncState;
            bytesRead = socket.EndSend(result);
            sendCompleted.Set();
        }

        public static JamPacket Receive(Socket socket)
        {
            ReceiveState state = new ReceiveState()
            {
                Socket = socket,
                Packet = new JamPacket(),
                HeaderBytes = new byte[Marshal.SizeOf(typeof(JamPacketHeader))]
            };
            socket.BeginReceive(state.HeaderBytes, 0, state.HeaderBytes.Length, 0, new AsyncCallback(ReceiveHeaderCallback), state);
            receiveCompleted.WaitOne();

            return state.Packet;
        }

        private static void ReceiveHeaderCallback(IAsyncResult result)
        {
            ReceiveState state = (ReceiveState)result.AsyncState;
            int bytesRead = state.Socket.EndReceive(result);

            if (bytesRead != Marshal.SizeOf(state.Packet.Header.GetType()))
                throw new InvalidOperationException("Did not read enough bytes to build a Header");

            state.Packet.Header = new JamPacketHeader(state.HeaderBytes);
            state.Packet.Data = new byte[state.Packet.Header.DataLength];
            state.Socket.BeginReceive(state.Packet.Data, 0, state.Packet.Header.DataLength, 0, new AsyncCallback(ReceiveDataCallback), state);
        }

        private static void ReceiveDataCallback(IAsyncResult result)
        {
            ReceiveState state = (ReceiveState)result.AsyncState;
            int bytesRead = state.Socket.EndReceive(result);

            if (bytesRead != state.Packet.Header.DataLength)
                throw new InvalidOperationException("Did not read enough bytes to fill data");

            receiveCompleted.Set();
        }
    }
}

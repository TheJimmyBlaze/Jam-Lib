using JamLib.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamLib.Server
{
    public class JamPacketRouter
    {
        public void Route(JamPacket packet, JamServerConnection senderConnection)
        {
            JamServer server = senderConnection.Server;

            Guid recipient = packet.Header.Receipient;
            Guid sender = packet.Header.Sender;
            
            if (recipient == Guid.Empty)
            {
                if (sender == Guid.Empty)
                    senderConnection.Login(packet);
                else
                    server.Interpreter.Interpret(packet);
            }
            else
            {
                JamServerConnection recipientConnection = server.GetConnection(recipient);

                if (recipientConnection == null)
                    HandleOfflineReceipient(packet);
                else
                    recipientConnection.Send(packet);
            }            
        }

        private virtual void HandleOfflineReceipient(JamPacket packet) { }
    }
}

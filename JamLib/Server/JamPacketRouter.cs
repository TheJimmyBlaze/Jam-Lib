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
                senderConnection.InternalInterpreter.Interpret(packet);
                return;
            }

            JamServerConnection recipientConnection = server.GetConnection(recipient);
            if (recipientConnection == null)
                HandleOfflineReceipient(packet);
            else
                recipientConnection.Send(packet);   
        }

        protected virtual void HandleOfflineReceipient(JamPacket packet) { }
    }
}

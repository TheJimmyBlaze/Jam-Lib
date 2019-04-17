using JamLib.Packet;
using JamLib.Packet.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamLib.Server
{
    public class InternalServerInterpreter : IJamPacketInterpreter
    {
        private readonly JamServerConnection serverConnection;

        public InternalServerInterpreter(JamServerConnection serverConnection)
        {
            this.serverConnection = serverConnection;
        }

        public void Interpret(JamPacket packet)
        {
            switch (packet.Header.DataType)
            {
                case LoginRequest.DATA_TYPE:
                    serverConnection.Login(packet);
                    break;
                case PingRequest.DATA_TYPE:
                    serverConnection.Ping(packet);
                    break;
                default:
                    serverConnection.Server.Interpreter.Interpret(packet);
                    break;
            }
        }
    }
}

using JamLib.Packet;
using JamLib.Packet.Data;
using JamLib.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleServer.Network
{
    internal static class ServerInterpreter
    {
        internal static void Interpret(JamServerConnection serverConnection, JamPacket packet)
        {
            switch (packet.Header.DataType)
            {
                case PlainTextImperative.DATA_TYPE:
                    WritePlainTextToConsole(packet);
                    break;
            }
        }

        private static void WritePlainTextToConsole(JamPacket packet)
        {
            PlainTextImperative plainText = new PlainTextImperative(packet.Data);
            Console.WriteLine("[{0}]: {1}", packet.Header.Sender, plainText.Text);
        }
    }
}

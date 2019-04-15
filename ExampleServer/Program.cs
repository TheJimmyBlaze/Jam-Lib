using JamLib.Database;
using JamLib.Domain;
using JamLib.Domain.Cryptography;
using JamLib.Packet;
using JamLib.Server;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleServer
{
    class Program
    {
        public static JamServer Server { get; private set; }

        private static int port = int.MinValue;
        private static string certificatePath = string.Empty;

        private static readonly ManualResetEvent serverExiting = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            if (!ReadArgs(args))
                return;

            IHashFactory hashFactory = BuildHashFactory();
            IJamPacketInterpreter interpreter = new ChatServerInterpreter();

            Server = new JamServer(hashFactory, interpreter);
            Server.Start(port, certificatePath);

            serverExiting.WaitOne();
        }

        private static bool ReadArgs(string[] args)
        {
            const string USAGE_EXAMPLE = "Usage: ExampleServer <port number as integer> <certificate path as string>";

            if (args.Length < 2)
            {
                Console.Error.WriteLine("ERROR: Incorrect number of arguments. {0}", USAGE_EXAMPLE);
                return false;
            }
            
            if (!int.TryParse(args[0], out port))
            {
                Console.Error.WriteLine("ERROR: Port (arg[0]) could not be converted to an integer. {0}", USAGE_EXAMPLE);
                return false;
            }

            certificatePath = args[1];
            if (!File.Exists(certificatePath))
            {
                Console.Error.WriteLine("ERROR: Certificate path: {0} does not exist. {1}", certificatePath, USAGE_EXAMPLE);
                return false;
            }

            return true;
        }

        private static IHashFactory BuildHashFactory()
        {
            const int MILLISECONDS_TO_SPEND_HASHING = 500;
            const string PEPPER_STRING = "This is a simple sample peper, come up with something better than this.";

            byte[] pepperBytes = Encoding.ASCII.GetBytes(PEPPER_STRING);
            SHA256HashFactory hashFactory = new SHA256HashFactory(MILLISECONDS_TO_SPEND_HASHING, pepperBytes);

            return hashFactory;
        }
    }
}

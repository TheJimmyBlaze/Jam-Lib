using ExampleServer.Network;
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
        private static string certificatePassword = string.Empty;

        private static readonly ManualResetEvent serverExiting = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            if (!LoadSettings())
                return;

            IHashFactory hashFactory = BuildHashFactory();

            EnsureRootAccount(hashFactory);

            Console.WriteLine("Starting server...");
            Server = new JamServer(hashFactory);

            Server.MessageReceivedEvent += ServerEventHandler.OnMessageReceived;

            Server.ClientConnectedEvent += ServerEventHandler.OnClientConnected;
            Server.ClientDisconnectedEvent += ServerEventHandler.OnClientDisconnect;

            Server.ClientIdentifiedEvent += ServerEventHandler.OnClientIdentified;
            Server.ClientInvalidUsernameEvent += ServerEventHandler.OnClientInvalidUsername;
            Server.ClientInvalidPasswordEvent += ServerEventHandler.OnClientInvalidPassword;

            Server.ClientConnectedElsewhereEvent += ServerEventHandler.OnClientConnectedElsewhere;
            
            Server.Start(port, certificatePath, certificatePassword);

            serverExiting.WaitOne();
        }

        private static bool LoadSettings()
        {
            port = Properties.Settings.Default.PortNumber;

            certificatePath = Properties.Settings.Default.CertificatePath;
            if (!File.Exists(certificatePath))
            {
                Console.Error.WriteLine("Settings Load ERROR: Certificate path: {0} does not exist.", certificatePath);
                return false;
            }

            certificatePassword = Properties.Settings.Default.CertificatePassword;

            return true;
        }

        private static void EnsureRootAccount(IHashFactory hashFactory)
        {
            if (!Domain.AccountFactory.RequireRootAccount())
                return;

            Console.WriteLine("A root account does not exist on the connected database, please create one now.");
            AttemptCreateRootAccount(hashFactory);
        }

        private static void AttemptCreateRootAccount(IHashFactory hashFactory)
        {
            Console.WriteLine("Enter root username:");
            string username = Console.ReadLine();

            Console.WriteLine("Enter root password:");
            string password = Console.ReadLine();

            try
            {
                Domain.AccountFactory.CreateRootAccount(username, password, hashFactory);
                Console.WriteLine("Root account created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Root Account ERROR: {0}", ex);
                AttemptCreateRootAccount(hashFactory);
            }
        }

        private static IHashFactory BuildHashFactory()
        {
            const int MILLISECONDS_TO_SPEND_HASHING = 500;
            const string PEPPER_STRING = "This is a simple sample peper, come up with something better than this.";

            byte[] pepperBytes = Encoding.ASCII.GetBytes(PEPPER_STRING);
            Sha256HashFactory hashFactory = new Sha256HashFactory(MILLISECONDS_TO_SPEND_HASHING, pepperBytes);

            return hashFactory;
        }
    }
}

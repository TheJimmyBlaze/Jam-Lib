using ExampleServer.Network;
using ExampleServer.Network.Data;
using JamLib.Database;
using JamLib.Domain;
using JamLib.Domain.Cryptography;
using JamLib.Domain.Serialization;
using JamLib.Packet;
using JamLib.Packet.Data;
using JamLib.Packet.DataRegisty;
using JamLib.Server;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleServer
{
    public class Program
    {
        public static string APP_SIGNITURE = "JamLib.ExampleServer";
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
            DataTypeRegistry prepopulatedRegistry = RegisterDataTypes(APP_SIGNITURE);
            Server = new JamServer(APP_SIGNITURE, prepopulatedRegistry, hashFactory, new Utf8JsonSerializer());

            Server.MessageReceivedEvent += ServerEventHandler.OnMessageReceived;

            Server.ClientConnectedEvent += ServerEventHandler.OnClientConnected;
            Server.ClientDisconnectedEvent += ServerEventHandler.OnClientDisconnect;

            Server.ClientIdentifiedEvent += ServerEventHandler.OnClientIdentified;
            Server.ClientInvalidUsernameEvent += ServerEventHandler.OnClientInvalidUsername;
            Server.ClientInvalidPasswordEvent += ServerEventHandler.OnClientInvalidPassword;

            Server.ClientConnectedElsewhereEvent += ServerEventHandler.OnClientConnectedElsewhere;
            Server.ClientOfflineAppRequest += ServerEventHandler.OnClientOfflineAppRequest;

            Server.DisposedEvent += ServerEventHandler.OnDisposed;
            
            Server.Start(port, certificatePath, certificatePassword);

            Console.WriteLine("Server started successfully.\n");

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
            try
            {
                if (AccountFactory.Count != 0)
                    return;

                Console.WriteLine("A root account does not exist on the connected database, please create one now.");
                AttemptCreateRootAccount(hashFactory);
            }
            catch (EntityException)
            {
                Console.WriteLine("A database connection could not be established.");
                Exit();
            }
        }

        private static void AttemptCreateRootAccount(IHashFactory hashFactory)
        {
            Console.WriteLine("Enter root username:");
            string username = Console.ReadLine();

            Console.WriteLine("Enter root password:");
            string password = Console.ReadLine();

            try
            {
                AccountFactory.Generate(username, password, hashFactory);
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

        private static DataTypeRegistry RegisterDataTypes(string appSigniture)
        {
            DataTypeRegistry registry = new DataTypeRegistry();

            registry.DataTypeRegisteredEvent += DataTypeRegistryEventHandler.OnDataTypeRegistered;
            registry.DataTypesDeregisteredEvent += DataTypeRegistryEventHandler.OnDataTypeDeregistered;

            List<DataType> dataTypes = new List<DataType>
            {
                new DataType(appSigniture, AccountOnlineStatusChangedImperative.DATA_SIGNITURE),
                new DataType(appSigniture, GetAccountsRequest.DATA_SIGNITURE),
                new DataType(appSigniture, GetAccountsResponse.DATA_SIGNITURE),
                new DataType(appSigniture, RegisterAccountRequest.DATA_SIGNITURE),
                new DataType(appSigniture, RegisterAccountResponse.DATA_SIGNITURE),
                new DataType(appSigniture, SendMessageImperative.DATA_SIGNITURE)
            };

            registry.BulkRegister(dataTypes);
            return registry;
        }

        public static void Exit()
        {
            Console.WriteLine("The program will now exit.");
            Environment.Exit(0);
        }
    }
}

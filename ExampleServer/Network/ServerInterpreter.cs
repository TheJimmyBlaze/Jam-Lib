﻿using ExampleServer.Network.Data;
using JamLib.Database;
using JamLib.Domain;
using JamLib.Packet;
using JamLib.Packet.Data;
using JamLib.Server;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
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
                case RegisterAccountRequest.DATA_TYPE:
                    RegisterAccount(serverConnection, packet);
                    break;
                case PlainTextImperative.DATA_TYPE:
                    WritePlainTextToConsole(serverConnection, packet);
                    break;
                case GetAccountsRequest.DATA_TYPE:
                    GetRegisteredAccounts(serverConnection);
                    break;
            }
        }

        private static void RegisterAccount(JamServerConnection serverConnection, JamPacket packet)
        {
            RegisterAccountRequest register = new RegisterAccountRequest(packet.Data, serverConnection.Serializer);

            RegisterAccountResponse response;
            try
            {
                Account createdAccount = AccountFactory.Generate(register.Username, register.Password, Program.Server.HashFactory, true);
                response = new RegisterAccountResponse(RegisterAccountResponse.AccountRegistrationResult.Good, createdAccount, serverConnection.Serializer);

                Console.WriteLine("Registered new Account: {0} - {1}", createdAccount.AccountID, createdAccount.Username);
            }
            catch (DbUpdateException)
            {
                response = new RegisterAccountResponse(RegisterAccountResponse.AccountRegistrationResult.Bad, null, serverConnection.Serializer);
            }

            JamPacket responsePacket = new JamPacket(Guid.Empty, Guid.Empty, RegisterAccountResponse.DATA_TYPE, response.GetBytes());
            serverConnection.Send(responsePacket);
        }

        private static void WritePlainTextToConsole(JamServerConnection serverConnection, JamPacket packet)
        {
            PlainTextImperative plainText = new PlainTextImperative(packet.Data, serverConnection.Serializer);
            Console.WriteLine("[{0}]: {1}", packet.Header.Sender, plainText.Text);
        }

        private static void GetRegisteredAccounts(JamServerConnection serverConnection)
        {
            using (JamServerEntities dbContext = new JamServerEntities())
            {
                dbContext.Configuration.LazyLoadingEnabled = false;
                List<Tuple<Account, bool>> accounts = new List<Tuple<Account, bool>>();

                foreach (Account account in dbContext.Accounts)
                {
                    bool online = serverConnection.Server.GetConnection(account.AccountID) != null;
                    accounts.Add(new Tuple<Account, bool>(account, online));
                }

                GetAccountsResponse response = new GetAccountsResponse(accounts, serverConnection.Serializer);

                JamPacket responsePacket = new JamPacket(Guid.Empty, Guid.Empty, GetAccountsResponse.DATA_TYPE, response.GetBytes());
                serverConnection.Send(responsePacket);
            }
        }
    }
}

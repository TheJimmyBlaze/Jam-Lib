using JamLib.Database;
using JamLib.Domain.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleServer.Network.Data
{
    public struct RegisterAccountResponse
    {
        public const int DATA_TYPE = 101;

        public enum AccountRegistrationResult { Good, Bad };

        public AccountRegistrationResult Result { get; set; }
        public Account Account { get; set; }

        private readonly ISerializer serializer;

        public RegisterAccountResponse(AccountRegistrationResult result, Account account, ISerializer serializer)
        {
            Result = result;
            Account = account;
            this.serializer = serializer;
        }

        public RegisterAccountResponse(byte[] rawBytes, ISerializer serializer)
        {
            this = serializer.GetStructFromBytes<RegisterAccountResponse>(rawBytes);
            this.serializer = serializer;
        }

        public byte[] GetBytes()
        {
            return serializer.GetBytesFromStruct(this);
        }
    }
}

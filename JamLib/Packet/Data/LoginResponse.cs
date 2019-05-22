using JamLib.Database;
using JamLib.Domain;
using JamLib.Domain.Serialization;
using JamLib.Packet.DataRegisty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamLib.Packet.Data
{
    public struct LoginResponse
    {
        public const int DATA_TYPE = 11;

        public enum LoginResult { Good, BadUsername, BadPassword, AppOffline };

        public LoginResult Result { get; set; }
        public Account Account { get; set; }
        public List<DataType> RegisteredDataTypes { get; set; }

        private readonly ISerializer serializer;

        public LoginResponse(LoginResult result, Account account, List<DataType> registeredDataTypes, ISerializer serializer)
        {
            Result = result;
            Account = account;
            RegisteredDataTypes = registeredDataTypes;
            this.serializer = serializer;
        }

        public LoginResponse(byte[] rawBytes, ISerializer serializer)
        {
            this = serializer.GetStructFromBytes<LoginResponse>(rawBytes);
            this.serializer = serializer;
        }

        public byte[] GetBytes()
        {
            return serializer.GetBytesFromStruct(this);
        }
    }
}

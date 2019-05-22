using JamLib.Database;
using JamLib.Domain.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleServer.Network.Data
{
    public struct AccountOnlineStatusChangedImperative
    {
        public const int DATA_TYPE = 107;
        public const string DATA_SIGNITURE = "AccountOnlineStatusChangeImperative";

        public Account Account { get; set; }
        public bool Online { get; set; }

        private readonly ISerializer serializer;

        public AccountOnlineStatusChangedImperative(Account account, bool online, ISerializer serializer)
        {
            Account = account;
            Online = online;
            this.serializer = serializer;
        }

        public AccountOnlineStatusChangedImperative(byte[] rawBytes, ISerializer serializer)
        {
            this = serializer.GetStructFromBytes<AccountOnlineStatusChangedImperative>(rawBytes);
            this.serializer = serializer;
        }

        public byte[] GetBytes()
        {
            return serializer.GetBytesFromStruct(this);
        }
    }
}

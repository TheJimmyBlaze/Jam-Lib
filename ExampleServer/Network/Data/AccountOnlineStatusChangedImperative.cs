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

        public Account Account { get; set; }
        public bool Online { get; set; }

        public AccountOnlineStatusChangedImperative(byte[] rawBytes)
        {
            this = Utf8JsonSerializer.GetStructFromBytes<AccountOnlineStatusChangedImperative>(rawBytes);
        }

        public byte[] GetBytes()
        {
            return Utf8JsonSerializer.GetBytesFromStruct(this);
        }
    }
}

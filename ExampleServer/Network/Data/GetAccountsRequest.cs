using JamLib.Domain.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleServer.Network.Data
{
    public struct GetAccountsRequest
    {
        public const int DATA_TYPE = 105;

        public GetAccountsRequest(byte[] rawBytes)
        {
            this = Utf8JsonSerializer.GetStructFromBytes<GetAccountsRequest>(rawBytes);
        }

        public byte[] GetBytes()
        {
            return Utf8JsonSerializer.GetBytesFromStruct(this);
        }
    }
}

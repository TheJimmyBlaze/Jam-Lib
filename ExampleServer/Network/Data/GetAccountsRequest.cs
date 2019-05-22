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
        public const string DATA_SIGNITURE = "GetAccountsRequest";

        private readonly ISerializer serializer;

        public GetAccountsRequest(ISerializer serializer)
        {
            this.serializer = serializer;
        }

        public GetAccountsRequest(byte[] rawBytes, ISerializer serializer)
        {
            this = serializer.GetStructFromBytes<GetAccountsRequest>(rawBytes);
            this.serializer = serializer;
        }

        public byte[] GetBytes()
        {
            return serializer.GetBytesFromStruct(this);
        }
    }
}

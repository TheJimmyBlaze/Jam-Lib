using JamLib.Database;
using JamLib.Domain.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleServer.Network.Data
{
    public struct GetAccountsResponse
    {
        public const int DATA_TYPE = 106;
        public const string DATA_SIGNITURE = "GetAccountsResponse";


        public List<Tuple<Account, bool>> Accounts { get; set; } //Tuple used to store online status of accompanied Account.

        private readonly ISerializer serializer;

        public GetAccountsResponse(List<Tuple<Account, bool>> accounts, ISerializer serializer)
        {
            Accounts = accounts;
            this.serializer = serializer;
        }

        public GetAccountsResponse(byte[] rawBytes, ISerializer serializer)
        {
            this = serializer.GetStructFromBytes<GetAccountsResponse>(rawBytes);
            this.serializer = serializer;
        }

        public byte[] GetBytes()
        {
            return serializer.GetBytesFromStruct(this);
        }
    }
}

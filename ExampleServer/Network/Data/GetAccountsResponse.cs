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
        
        public List<Tuple<Account, bool>> Accounts; //Tuple used to store online status of accompanied Account.

        public GetAccountsResponse(byte[] rawBytes)
        {
            this = Utf8JsonSerializer.GetStructFromBytes<GetAccountsResponse>(rawBytes);
        }
        
        public byte[] GetBytes()
        {
            return Utf8JsonSerializer.GetBytesFromStruct(this);
        }
    }
}

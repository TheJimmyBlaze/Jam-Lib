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

        public RegisterAccountResponse(byte[] rawBytes)
        {
            this = Utf8JsonSerializer.GetStructFromBytes<RegisterAccountResponse>(rawBytes);
        }

        public byte[] GetBytes()
        {
            return Utf8JsonSerializer.GetBytesFromStruct(this);
        }
    }
}

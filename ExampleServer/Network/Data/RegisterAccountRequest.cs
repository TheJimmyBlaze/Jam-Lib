using JamLib.Domain.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleServer.Network.Data
{
    public struct RegisterAccountRequest
    {
        public const int DATA_TYPE = 100;

        public string Username { get; set; }
        public string Password { get; set; }

        public RegisterAccountRequest(byte[] rawBytes)
        {
            this = Utf8JsonSerializer.GetStructFromBytes<RegisterAccountRequest>(rawBytes);
        }

        public byte[] GetBytes()
        {
            return Utf8JsonSerializer.GetBytesFromStruct(this);
        }
    }
}

using JamLib.Domain;
using JamLib.Domain.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JamLib.Packet.Data
{
    public struct LoginRequest
    {
        public const int DATA_TYPE = 10;

        public string Username { get; set; }
        public string Password { get; set; }

        public LoginRequest(byte[] rawBytes)
        {
            this = Utf8JsonSerializer.GetStructFromBytes<LoginRequest>(rawBytes);
        }

        public byte[] GetBytes()
        {
            return Utf8JsonSerializer.GetBytesFromStruct(this);
        }
    }
}

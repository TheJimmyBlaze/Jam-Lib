using JamLib.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamLib.Packet.Data
{
    public struct LoginResponse
    {
        public const int DATA_TYPE = 2;

        public enum LoginResult { Good, BadUsername, BadPassword };

        public LoginResult Result { get; set; }
        public JamAccount Account { get; set; }

        public LoginResponse(byte[] rawBytes)
        {
            this = StructMarshal.GetStructFromBytes<LoginResponse>(rawBytes);
        }

        public byte[] GetBytes()
        {
            return StructMarshal.GetBytesFromStruct(this);
        }
    }
}

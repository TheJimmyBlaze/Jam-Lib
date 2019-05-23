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
        public const string DATA_SIGNITURE = "LoginRequest";

        public string AppSigniture { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        private readonly ISerializer serializer;

        public LoginRequest(string appSigniture, string username, string password, ISerializer serializer)
        {
            AppSigniture = appSigniture;
            Username = username;
            Password = password;
            this.serializer = serializer;
        }

        public LoginRequest(byte[] rawBytes, ISerializer serializer)
        {
            this = serializer.GetStructFromBytes<LoginRequest>(rawBytes);
            this.serializer = serializer;
        }

        public byte[] GetBytes()
        {
            return serializer.GetBytesFromStruct(this);
        }
    }
}

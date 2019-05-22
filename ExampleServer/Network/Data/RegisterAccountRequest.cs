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
        public const string DATA_SIGNITURE = "RegisterAccountRequest";

        public string Username { get; set; }
        public string Password { get; set; }

        private readonly ISerializer serializer;

        public RegisterAccountRequest(string username, string password, ISerializer serializer)
        {
            Username = username;
            Password = password;
            this.serializer = serializer;
        }

        public RegisterAccountRequest(byte[] rawBytes, ISerializer serializer)
        {
            this = serializer.GetStructFromBytes<RegisterAccountRequest>(rawBytes);
            this.serializer = serializer;
        }

        public byte[] GetBytes()
        {
            return serializer.GetBytesFromStruct(this);
        }
    }
}

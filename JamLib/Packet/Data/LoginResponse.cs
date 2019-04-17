﻿using JamLib.Database;
using JamLib.Domain;
using JamLib.Domain.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamLib.Packet.Data
{
    public struct LoginResponse
    {
        public const int DATA_TYPE = 11;

        public enum LoginResult { Good, BadUsername, BadPassword };

        public LoginResult Result { get; set; }
        public Account Account { get; set; }

        public LoginResponse(byte[] rawBytes)
        {
            this = Utf8JsonSerializer.GetStructFromBytes<LoginResponse>(rawBytes);
        }

        public byte[] GetBytes()
        {
            return Utf8JsonSerializer.GetBytesFromStruct(this);
        }
    }
}

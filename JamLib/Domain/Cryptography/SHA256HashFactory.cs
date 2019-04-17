using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JamLib.Domain.Cryptography
{
    public class Sha256HashFactory : IHashFactory
    {
        private struct MetaData
        {
            public int HashingIterations { get; set; }
            public int HashLength { get; set; }

            public MetaData(byte[] bytes)
            {
                HashingIterations = 0;
                HashLength = 0;

                int size = Marshal.SizeOf(this);
                IntPtr pointer = Marshal.AllocHGlobal(size);

                Marshal.Copy(bytes, 0, pointer, size);
                this = (MetaData)Marshal.PtrToStructure(pointer, GetType());
                Marshal.FreeHGlobal(pointer);
            }

            public byte[] GetBytes()
            {
                int size = Marshal.SizeOf(this);
                byte[] bytes = new byte[size];

                IntPtr pointer = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(this, pointer, true);
                Marshal.Copy(pointer, bytes, 0, size);
                Marshal.FreeHGlobal(pointer);

                return bytes;
            }
        }

        private const int STORABLE_HASH_LENGTH = 128;

        private readonly byte[] pepper;
        private readonly int millisecondsToSpendHashing;

        public Sha256HashFactory(int millisecondsToSpendHashing, byte[] pepper)
        {
            this.pepper = pepper;
            this.millisecondsToSpendHashing = millisecondsToSpendHashing;
        }

        private byte[] GenerateSalt(int length)
        {
            using (RNGCryptoServiceProvider random = new RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[length];
                random.GetBytes(salt);

                return salt;
            }
        }

        private byte[] AddSeasoning(byte[] password, byte[] salt)
        {
            byte[] seasonedPassword = new byte[password.Length + salt.Length + pepper.Length];

            Array.Copy(pepper, seasonedPassword, pepper.Length);
            Array.Copy(password, 0, seasonedPassword, pepper.Length, password.Length);
            Array.Copy(salt, 0, seasonedPassword, pepper.Length + password.Length, salt.Length);

            return seasonedPassword;
        }

        private byte[] GetStorableHash(byte[] hash, byte[] salt, int hashingIterations)
        {
            MetaData metaData = new MetaData()
            {
                HashLength = hash.Length,
                HashingIterations = hashingIterations
            };

            byte[] metaBytes = metaData.GetBytes();
            byte[] combination = new byte[metaBytes.Length + hash.Length + salt.Length];

            Array.Copy(metaBytes, combination, metaBytes.Length);
            Array.Copy(hash, 0, combination, metaBytes.Length, hash.Length);
            Array.Copy(salt, 0, combination, metaBytes.Length + hash.Length, salt.Length);

            return combination;
        }

        public byte[] BuildHash(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.ASCII.GetBytes(password);
                int hashSizeInBytes = sha.HashSize / 8;
                byte[] salt = GenerateSalt(STORABLE_HASH_LENGTH - hashSizeInBytes - Marshal.SizeOf(new MetaData()));

                byte[] hash = AddSeasoning(passwordBytes, salt);

                DateTime stopHashing = DateTime.UtcNow + new TimeSpan(0, 0, 0, 0, millisecondsToSpendHashing);
                int hashingIterations = 0;
                while (DateTime.UtcNow < stopHashing)
                {
                    hash = sha.ComputeHash(hash);
                    hashingIterations++;
                }

                return GetStorableHash(hash, salt, hashingIterations);
            }
        }

        public bool ValidateString(string value, byte[] storableHash)
        {
            using (SHA256 sha = SHA256.Create())
            {
                int metaDataLength = Marshal.SizeOf(new MetaData());
                MetaData metaData = new MetaData(storableHash.Take(metaDataLength).ToArray());
                
                byte[] hash = storableHash.Skip(metaDataLength).Take(metaData.HashLength).ToArray();
                byte[] salt = storableHash.Skip(metaDataLength + metaData.HashLength).ToArray();

                byte[] valueBytes = Encoding.ASCII.GetBytes(value);
                byte[] valueHash = AddSeasoning(valueBytes, salt);

                for (int i = 0; i < metaData.HashingIterations; i++)
                {
                    valueHash = sha.ComputeHash(valueHash);
                }

                byte[] comparableHash = GetStorableHash(valueHash, salt, metaData.HashingIterations);
                if (comparableHash.SequenceEqual(storableHash))
                    return true;
                return false;
            }
        }
    }
}

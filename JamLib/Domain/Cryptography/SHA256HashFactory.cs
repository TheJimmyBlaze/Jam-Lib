using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JamLib.Domain.Cryptography
{
    public class SHA256HashFactory : IHashFactory
    {
        private const int STORABLE_HASH_LENGTH = 128;
        private const int HASH_LENGTH = 32;
        private const int META_DATA_LENGTH = 4;

        private readonly byte[] pepper;
        private readonly int hashingItterations;

        public SHA256HashFactory(int hashingItterations, byte[] pepper)
        {
            if (hashingItterations > Math.Pow(2, 24))
                throw new ArgumentException(string.Format("Too many hasingItterations, must be less than {0}", Math.Pow(2, 24)));

            this.pepper = pepper;
            this.hashingItterations = hashingItterations;
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

        private byte[] GetMetaData(byte[] hash)
        {
            byte[] metaData = new byte[4];

            byte[] itterations = BitConverter.GetBytes(hashingItterations);
            Array.Copy(itterations, metaData, itterations.Length);

            byte hashLength = (byte)hash.Length;
            metaData[3] = hashLength;

            return metaData;
        }

        private byte[] GetStorableHash(byte[] hash, byte[] salt)
        {
            if (hash.Length > byte.MaxValue)
                throw new ArgumentException(string.Format("Hash is too long, can be maximum of {0} bytes", byte.MaxValue));

            byte[] metaData = GetMetaData(hash);
            byte[] combination = new byte[metaData.Length + hash.Length + salt.Length];

            Array.Copy(metaData, combination, metaData.Length);
            Array.Copy(hash, 0, combination, metaData.Length, hash.Length);
            Array.Copy(salt, 0, combination, metaData.Length + hash.Length, salt.Length);

            return combination;
        }

        public byte[] BuildHash(string password)
        {
            byte[] passwordBytes = Encoding.ASCII.GetBytes(password);
            byte[] salt = GenerateSalt(STORABLE_HASH_LENGTH - META_DATA_LENGTH - HASH_LENGTH);

            byte[] hash = null;
            using (SHA256 sha = SHA256.Create())
            {
                for (int i = 0; i < hashingItterations; i++)
                {
                    if (hash == null)
                        hash = sha.ComputeHash(AddSeasoning(passwordBytes, salt));

                    hash = sha.ComputeHash(hash);
                }
            }

            return GetStorableHash(hash, salt);
        }

        public bool ValidateString(string value, byte[] storableHash)
        {
            int hashLength = storableHash[0];
            byte[] hash = storableHash.Skip(META_DATA_LENGTH).Take(hashLength).ToArray();
            byte[] salt = storableHash.Skip(META_DATA_LENGTH + hashLength).ToArray();

            byte[] valueBytes = Encoding.ASCII.GetBytes(value);
            byte[] valueHash = null;
            using (SHA256 sha = SHA256.Create())
            {
                for (int i = 0; i < hashingItterations; i++)
                {
                    if (valueHash == null)
                        valueHash = sha.ComputeHash(AddSeasoning(valueBytes, salt));

                    valueHash = sha.ComputeHash(valueHash);
                }
            }

            byte[] comparableHash = GetStorableHash(valueHash, salt);
            if (comparableHash.SequenceEqual(storableHash))
                return true;
            return false;
        }
    }
}

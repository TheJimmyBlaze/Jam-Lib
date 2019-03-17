using JamLib.Database;
using JamLib.Domain.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamLib.Domain
{
    public class JamAccount: Account
    {
        public JamAccount(string username, string password, IHashFactory hashFactory, bool approved = false)
        {
            AccountID = Guid.NewGuid();
            LastUpdateUTC = DateTime.UtcNow;

            Username = username;
            Approved = approved;

            AccountAccessCode accessCode = new AccountAccessCode()
            {
                AccountAccessCodeID = Guid.NewGuid(),
                LastUpdateUTC = DateTime.UtcNow,

                AccountID = AccountID,
                AccessCode = hashFactory.BuildHash(password)
            };

            AccountAccessCodes.Add(accessCode);
        }
    }
}

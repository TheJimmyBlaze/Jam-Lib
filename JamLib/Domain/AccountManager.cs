using JamLib.Database;
using JamLib.Domain.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace JamLib.Domain
{
    public static class AccountManagement
    {
        public static Account CreateAccount(string userName, string password, IHashFactory hashFactory, bool approved = false)
        {
            Account account = new Account()
            {
                AccountID = Guid.NewGuid(),
                LastUpdateUTC = DateTime.UtcNow,

                UserName = userName,
                Approved = approved
            };
            
            AccountAccessCode accessCode = new AccountAccessCode()
            {
                AccountAccessCodeID = Guid.NewGuid(),
                LastUpdateUTC = DateTime.UtcNow,

                AccountID = account.AccountID,
                AccessCode = hashFactory.BuildHash(password)
            };

            account.AccountAccessCodes.Add(accessCode);
            return account;
        }
    }
}

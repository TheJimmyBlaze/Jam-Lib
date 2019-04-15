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
        public class InvalidUsernameException: Exception
        {
            public InvalidUsernameException() { }
        }

        public class InvalidAccessCodeException: Exception
        {
            public InvalidAccessCodeException() { }
        }

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

        public static JamAccount Authenticate(string username, string password, IHashFactory hashFactory)
        {
            JamServerEntities context = new JamServerEntities();
            Account account = context.Accounts.SingleOrDefault(x => x.Username == username);

            if (account != null)
            {
                AccountAccessCode accessCode = context.AccountAccessCodes.SingleOrDefault(x => x.AccountID == account.AccountID);

                if (accessCode != null && hashFactory.ValidateString(password, accessCode.AccessCode))
                {
                    return (JamAccount)account;
                }
                else throw new InvalidAccessCodeException();
            }
            else throw new InvalidUsernameException();
        }
    }
}

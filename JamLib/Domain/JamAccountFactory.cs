using JamLib.Database;
using JamLib.Domain.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamLib.Domain
{
    public class JamAccountFactory
    {
        public class InvalidUsernameException: Exception
        {
            public InvalidUsernameException() { }
        }

        public class InvalidAccessCodeException: Exception
        {
            public InvalidAccessCodeException() { }
        }

        public static Account Generate(string username, string password, IHashFactory hashFactory, bool approved = false)
        {
            Account account = new Account()
            {
                AccountID = Guid.NewGuid(),
                LastUpdateUTC = DateTime.UtcNow,

                Username = username,
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

            JamServerEntities dbContext = new JamServerEntities();
            dbContext.Accounts.Add(account);
            dbContext.SaveChanges();

            return account;
        }

        public static Account Authenticate(string username, string password, IHashFactory hashFactory)
        {
            JamServerEntities context = new JamServerEntities();
            context.Configuration.LazyLoadingEnabled = false;
            Account account = context.Accounts.SingleOrDefault(x => x.Username == username);

            if (account != null)
            {
                AccountAccessCode accessCode = context.AccountAccessCodes.SingleOrDefault(x => x.AccountID == account.AccountID);

                if (accessCode != null && hashFactory.ValidateString(password, accessCode.AccessCode))
                {
                    return account;
                }
                else throw new InvalidAccessCodeException();
            }
            else throw new InvalidUsernameException();
        }
    }
}

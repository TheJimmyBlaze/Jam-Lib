using JamLib.Database;
using JamLib.Domain.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamLib.Domain
{
    public class AccountFactory
    {
        #region Exceptions
        public class InvalidUsernameException: Exception
        {
            public InvalidUsernameException() { }
        }

        public class InvalidAccessCodeException: Exception
        {
            public InvalidAccessCodeException() { }
        }
        #endregion

        public static int Count
        {
            get
            {
                using (JamServerEntities dbContext = new JamServerEntities())
                {
                    return dbContext.Accounts.Count();
                }
            }
        }

        public static List<Account> Accounts
        {
            get
            {
                using (JamServerEntities dbContext = new JamServerEntities())
                {
                    List<Account> accounts = new List<Account>();
                    foreach(Account account in dbContext.Accounts)
                    {
                        account.AccountAccessCodes.Clear();
                        accounts.Add(account);
                    }
                    return accounts;
                }
            }
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

            using (JamServerEntities dbContext = new JamServerEntities())
            {
                dbContext.Accounts.Add(account);
                dbContext.SaveChanges();
            }

            account.AccountAccessCodes.Clear();
            return account;
        }

        public static Account Authenticate(string username, string password, IHashFactory hashFactory)
        {
            using (JamServerEntities dbContext = new JamServerEntities())
            {
                Account account = dbContext.Accounts.SingleOrDefault(x => x.Username == username);

                if (account != null)
                {
                    AccountAccessCode accessCode = dbContext.AccountAccessCodes.SingleOrDefault(x => x.AccountID == account.AccountID);

                    if (accessCode != null && hashFactory.ValidateString(password, accessCode.AccessCode))
                    {
                        account.AccountAccessCodes.Clear();
                        return account;
                    }
                    else throw new InvalidAccessCodeException();
                }
                else throw new InvalidUsernameException();
            }
        }
    }
}

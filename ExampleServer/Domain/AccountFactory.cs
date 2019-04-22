using JamLib.Database;
using JamLib.Domain.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleServer.Domain
{
    internal class AccountFactory
    {
        public static bool RequireRootAccount()
        {
            using (JamServerEntities dbContext = new JamServerEntities())
            {
                if (dbContext.Accounts.Count() == 0)
                    return true;
            }
            return false;
        }

        public static void CreateRootAccount(string username, string password, IHashFactory hashFactory)
        {
            if (!RequireRootAccount())
                throw new InvalidOperationException("Cannot create root account as one already exists.");

            JamLib.Domain.AccountFactory.Generate(username, password, hashFactory, true);
        }
    }
}

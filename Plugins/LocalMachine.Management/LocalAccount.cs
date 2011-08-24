using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using log4net;
using System.DirectoryServices;

namespace pGina.Plugin.LocalMachine.Management
{
    class LocalAccount
    {
        static ILog m_logger = LogManager.GetLogger("LocalAccount");

        public static void Create(pGina.Shared.Types.UserInformation userInfo)
        {
            // Local machine
            using (DirectoryEntry local = new DirectoryEntry("WinNT://localhost"))
            {
                bool userExists = false;

                foreach (DirectoryEntry ent in local.Children)
                {
                    userExists = ent.Name.Equals(userInfo.Username, StringComparison.CurrentCultureIgnoreCase);
                    if (userExists)
                        break;
                }

                if (!userExists)
                {
                    m_logger.InfoFormat("Creating local user {0}", userInfo.Username);
                    using (DirectoryEntry user = local.Children.Add(userInfo.Username, "User"))
                    {
                        user.Properties["FullName"].Add(String.Format("{0} (pGina)", userInfo.Username));
                        user.Invoke("SetPassword", userInfo.Password);
                        // Possibly set some flags here...
                        // user.Invoke("Put", new object[] {"UserFlags", ...});
                        user.CommitChanges();
                    }
                }
                else
                {
                    m_logger.InfoFormat("Account {0} already exists, skipping.", userInfo.Username);
                }
            }

            // TODO: create/load local profile?
        }

        public static void Delete(string userName)
        {
            m_logger.DebugFormat("Deleting user {0}", userName);
            using( DirectoryEntry local = new DirectoryEntry("WinNT://localhost"))
            {
                using (DirectoryEntry user = local.Children.Find(userName))
                {
                    if (user != null)
                    {
                        local.Children.Remove(user);
                    }
                }
            }

            // TODO: Delete local profile?
        }
    }
}

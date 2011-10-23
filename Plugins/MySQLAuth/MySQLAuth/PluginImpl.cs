/*
	Copyright (c) 2011, pGina Team
	All rights reserved.

	Redistribution and use in source and binary forms, with or without
	modification, are permitted provided that the following conditions are met:
		* Redistributions of source code must retain the above copyright
		  notice, this list of conditions and the following disclaimer.
		* Redistributions in binary form must reproduce the above copyright
		  notice, this list of conditions and the following disclaimer in the
		  documentation and/or other materials provided with the distribution.
		* Neither the name of the pGina Team nor the names of its contributors 
		  may be used to endorse or promote products derived from this software without 
		  specific prior written permission.

	THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
	ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
	WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
	DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY
	DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
	(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
	LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
	ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
	(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
	SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

using pGina.Shared.Interfaces;

using MySql.Data.MySqlClient;

namespace pGina.Plugin.MySQLAuth
{
    public class PluginImpl : IPluginAuthentication, IPluginConfiguration
    {
        public static readonly Guid PluginUuid = new Guid("{A89DF410-53CA-4FE1-A6CA-4479B841CA19}");
        private ILog m_logger = LogManager.GetLogger("MySQLAuth");

        public Shared.Types.BooleanResult AuthenticateUser(Shared.Types.SessionProperties properties)
        {
            Shared.Types.UserInformation userInfo = properties.GetTrackedSingle<Shared.Types.UserInformation>();

            m_logger.DebugFormat("Authenticate: {0}", userInfo.Username);

            UserEntry entry = GetUserEntry(userInfo.Username);

            if (entry != null)
            {
                bool passwordOk = entry.VerifyPassword(userInfo.Password);
                if (passwordOk)
                {
                    m_logger.DebugFormat("Authentication successful for {0}", userInfo.Username);
                    return new Shared.Types.BooleanResult() { Success = true, Message = "Success." };
                }
                else
                {
                    m_logger.DebugFormat("Authentication failed for {0}", userInfo.Username); 
                    return new Shared.Types.BooleanResult() { Success = false, Message = "Invalid username or password." };
                }
            }
            else
            {
                m_logger.DebugFormat("Authentication failed for {0} no entry found in DB.", userInfo.Username);
                return new Shared.Types.BooleanResult() { Success = false, Message = "Invalid username or password." };
            }
        }

        public string Description
        {
            get { return "Authenticates users using a MySQL server as the account database."; }
        }

        public string Name
        {
            get { return "MySQL Authentication"; }
        }

        public void Starting()
        {
            
        }

        public void Stopping()
        {
            
        }

        public Guid Uuid
        {
            get { return PluginUuid; }
        }

        public string Version
        {
            get 
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public void Configure()
        {
            Configuration dialog = new Configuration();
            dialog.ShowDialog();
        }

        private UserEntry GetUserEntry(string user)
        {
            m_logger.Debug("GetUserEntry");

            MySqlConnectionStringBuilder bldr = new MySqlConnectionStringBuilder();
            bldr.Server = Settings.Store.Host;
            int port = Settings.Store.Port;
            bldr.Port = Convert.ToUInt32(port);
            bldr.UserID = Settings.Store.User;
            bldr.Database = Settings.Store.Database;
            bldr.Password = Settings.Store.GetEncryptedSetting("Password");
            bool useSsl = Settings.Store.UseSsl;
            if (useSsl)
                bldr.SslMode = MySqlSslMode.Required;
            string tableName = Settings.Store.Table;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(bldr.GetConnectionString(true)))
                {
                    conn.Open();

                    string query = string.Format("SELECT user, hash_method, password " +
                        "FROM {0} WHERE user=@user", tableName);
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@user", user);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                    {
                        rdr.Read();
                        UserEntry entry = new UserEntry()
                        {
                            Name = rdr[0].ToString(),
                            HashedPassword = rdr[2].ToString()
                        };
                        switch (rdr[1].ToString())
                        {
                            case "NONE":
                                entry.HashAlg = PasswordHashAlgorithm.NONE;
                                break;
                            case "MD5":
                                entry.HashAlg = PasswordHashAlgorithm.MD5;
                                break;
                            case "SHA1":
                                entry.HashAlg = PasswordHashAlgorithm.SHA1;
                                break;
                            case "SHA256":
                                entry.HashAlg = PasswordHashAlgorithm.SHA256;
                                break;
                            case "SHA512":
                                entry.HashAlg = PasswordHashAlgorithm.SHA512;
                                break;
                            case "SHA384":
                                entry.HashAlg = PasswordHashAlgorithm.SHA384;
                                break;
                            default:
                                m_logger.ErrorFormat("Unrecognized hash algorithm: {0}", rdr[1].ToString());
                                return null;
                        }
                        rdr.Close();
                        return entry;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (MySqlException ex)
            {
                m_logger.Error(ex.ToString());
                return null;
            }
        }
    }
}

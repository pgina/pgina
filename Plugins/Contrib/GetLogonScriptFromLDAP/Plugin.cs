/*
    Written by Florian Rohmer (2013)
     
    Distribued under the pGina license.
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
using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.IO;
using System.ServiceProcess;
using System.Web;
using System.ComponentModel;
using System.Runtime.InteropServices;

using pGina.Shared.Types;
using pGina.Shared.Interfaces;
using log4net;

namespace pGina.Plugin.LogonScriptFromLDAP
{
    public class PluginImpl : IPluginConfiguration, IPluginAuthenticationGateway, IPluginEventNotifications 
    {
        /// <summary>
        /// logger for PluginImpl
        /// </summary>
        private ILog pluginImpl_logger = LogManager.GetLogger("LogonScript_PluginImpl");

        /// <summary>
        /// Name (found on LDAP) of the script file
        /// </summary>
        private string scriptName;

        public static readonly Guid LdapLogonScript_uuid = new Guid("A26FEFFF-1753-4E81-9782-12D3C779ABEA");

        public PluginImpl()
        {
            using (Process me = Process.GetCurrentProcess())
            {
                pluginImpl_logger.DebugFormat("LDAP LogonScript Plugin initialized on {0} in PID: {1} Session: {2}", 
                    Environment.MachineName, me.Id, me.SessionId);
            }
        }

        public string Name
        {
            get { return "LogonScript from LDAP"; }
        }

        public string Description
        {
            get { return "Connects to a LDAP server according to given credentials. Gets logon script's name from it and then runs the corresponding script."; }
        }

        public Guid Uuid
        {
            get { return LdapLogonScript_uuid; }
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
            Configuration config = new Configuration();
            config.ShowDialog();
        }

        /// <summary>
        /// Stores a copy of user's login and password in SessionProperties.properties
        /// so that we will still have access to them after Single User plugin (if used)
        /// </summary>
        public BooleanResult AuthenticatedUserGateway(Shared.Types.SessionProperties properties)
        {
            pluginImpl_logger.DebugFormat("Authenticated User Gateway.");
            Shared.Types.UserInformation userInfo = properties.GetTrackedSingle<Shared.Types.UserInformation>();
            properties.AddTracked("UserLogin",    userInfo.Username);
            properties.AddTracked("UserPassword", userInfo.Password);
            pluginImpl_logger.DebugFormat("Login copy & password copy successfully stored in SessionProperties.properties.");

            return new BooleanResult { Success = true, Message = "Login & password successfully stored in properties." };
        }

        /// <summary>
        /// Retrieves the name of the logon script file (LDAP part), 
        /// and executes it from the server (server part).
        /// </summary>
        public void SessionChange(SessionChangeDescription changeDescription, SessionProperties properties)
        {
            if (changeDescription.Reason.Equals(System.ServiceProcess.SessionChangeReason.SessionLogon)) // checking if we just logged in
            {
                LdapPart(changeDescription, properties);
                ServerPart(changeDescription, properties);
            }
       }

        /// <summary>
        /// Connects to LDAP Server according to user's credentials.
        /// (These credentials have been stored in the SessionProperties object
        /// during the Gateway stage.)
        /// Retrieves the name of the script file on the user's LDAP account.
        /// </summary>
        private void LdapPart(SessionChangeDescription changeDescription, SessionProperties properties) {
            // initializes and sets up a new Ldap connection
            LdapInitialization(properties);
            // Get the LdapServer object from the session properties (created in LdapInitialization)
            LdapServer server = properties.GetTrackedSingle<LdapServer>();

            if (server == null)
            {
                pluginImpl_logger.ErrorFormat("Internal error: LdapServer object not available.");
                return;
            }

            try
            {
                pluginImpl_logger.DebugFormat("AuthenticateUser({0})", properties.Id.ToString());

                // retrieving user's information stored during Gateway stage
                Shared.Types.UserInformation userInfo = properties.GetTrackedSingle<Shared.Types.UserInformation>();
                string userLogin = properties.GetTracked<string>("UserLogin");
                string userPassword = properties.GetTracked<string>("UserPassword");
                pluginImpl_logger.DebugFormat("Received username: {0}", userLogin);

                // Authenticate the login
                pluginImpl_logger.DebugFormat("Attempting authentication for {0}", userLogin);
                BooleanResult authenticateBool = server.Authenticate(userLogin, userPassword);

                if (!authenticateBool.Success) // authentication and attribute value retrieving didn't work
                {
                    pluginImpl_logger.ErrorFormat("LDAP Authentication failed. {0}" , authenticateBool.Message);
                    return;
                }

                // retrieves the script name from Ldap
                this.scriptName = server.GetScriptName();
                pluginImpl_logger.DebugFormat("Name of the script file:  {0}",this.scriptName);

                // cleans up any resources held by the plugin
                LdapEnd(properties);
            }
            catch (Exception e)
            {
                if (e is LdapException)
                {
                    LdapException ldapEx = (e as LdapException);

                    if (ldapEx.ErrorCode == 81)
                    {
                        // Server can't be contacted, set server object to null
                        pluginImpl_logger.ErrorFormat("Server unavailable: {0}, {1}", ldapEx.ServerErrorMessage, e.Message);
                        server.Close();
                        properties.AddTrackedSingle<LdapServer>(null);
                        return;
                    }
                }

                // This is an unexpected error, so set LdapServer object to null, because
                // subsequent stages shouldn't use it, and this indicates to later stages
                // that this stage failed unexpectedly.
                server.Close();
                properties.AddTrackedSingle<LdapServer>(null);
                pluginImpl_logger.ErrorFormat("Exception in LDAP authentication: {0}", e);
                throw;  // Allow pGina service to catch and handle exception
            }
        }

        /// <summary>
        /// If we are not already connected, this connects to the given-in-config server using user's credentials.
        /// Finds the script file according to the name found on LDAP, and executes each line.
        /// </summary>
        private void ServerPart(SessionChangeDescription changeDescription, SessionProperties properties)
        {
            try
            {
                /* address of the server to connect to */
                string remoteUNC = Settings.Store.Server;
                /* file to run */
                string fileToRun = remoteUNC + @"\" + this.scriptName;

                // It is possible that we already have access to the file : if so, we don't connect
                if (!File.Exists(fileToRun)) // we could not reach the file, so we connect
                {
                    /* connection to the server using user's credentials */
                    string userLogin = properties.GetTracked<string>("UserLogin");
                    string userPassword = properties.GetTracked<string>("UserPassword");

                    pluginImpl_logger.DebugFormat("Connection to [" + remoteUNC + "] with credentials given in the first user authentication.");
                    string connectionFailure = PinvokeWindowsNetworking.connectToRemote(remoteUNC, userLogin, userPassword);

                    if (connectionFailure == null) // No error returned
                        pluginImpl_logger.DebugFormat("Successful connection.");
                    else // an error occured
                    {
                        pluginImpl_logger.ErrorFormat("Connection error. " + connectionFailure + " The scriptfile was not executed.");
                        return;
                    }
                }
                else pluginImpl_logger.DebugFormat("Already have access to [{0}]. No need to connect.", remoteUNC);

                ScriptExecution(changeDescription, properties, fileToRun);

                /* Server disconnection */
                string disconnectionFailure = PinvokeWindowsNetworking.disconnectRemote(remoteUNC);

                if (disconnectionFailure == null) // No error returned
                    pluginImpl_logger.DebugFormat("Successful server disconnection.");
                else // an error occured
                {
                    pluginImpl_logger.ErrorFormat("Server disconnection error. " + disconnectionFailure);
                }

            }
            catch (Exception e)
            {
                pluginImpl_logger.DebugFormat(e.Message);
            }
        }

        /// <summary>
        /// Finds the script file on the server and executes each line.
        /// </summary>
        private void ScriptExecution(SessionChangeDescription changeDescription, SessionProperties properties, string fileToRun)
        {
             if (!File.Exists(fileToRun)) // nonexistent file
             {
                 pluginImpl_logger.ErrorFormat("The file {0} does not exist!", fileToRun);
                 return;
             }

             pluginImpl_logger.DebugFormat("Will execute each line (except if it starts with rem or @rem) of {0}", fileToRun);
             string[] lines = System.IO.File.ReadAllLines(@fileToRun);

             foreach (string line in lines)
             {
                 StringBuilder lineCopy = new StringBuilder(line);

                 if (line.ToLower().Trim().Length == 0) pluginImpl_logger.DebugFormat("Command line is empty. We skip it.");
                 else if (!(line.ToLower().Trim().StartsWith("@rem") || line.ToLower().Trim().StartsWith("rem")))
                 {   // we have a command

                     pluginImpl_logger.DebugFormat("Command line : executing {0}", line);

                     if ((line.ToLower().Trim().Contains("net use")))
                     { // we have a net use command, so we add the username and password at the end.

                         string windowsDomain = Settings.Store.Domain + @"\";

                         // we get back user's information (stored during Gateway stage) 
                         Shared.Types.UserInformation userInfo = properties.GetTrackedSingle<Shared.Types.UserInformation>();
                         string userLogin = properties.GetTracked<string>("UserLogin");
                         string userPassword = properties.GetTracked<string>("UserPassword");

                         string toAppend = " /user:" + windowsDomain + userLogin + " " + userPassword;
                         lineCopy.Append(toAppend);
                     }

                     try
                     {
                         pInvokes.StartUserProcessInSession(changeDescription.SessionId, 
                                                            Environment.GetEnvironmentVariable("comspec"), 
                                                            lineCopy.Insert(0, "/C ").ToString());
                         // we insert a /c at index 0 of the command line so that cmd.exe understands he has to execute it and close the terminal
                     }
                     catch (Win32Exception e)
                     {
                         pluginImpl_logger.DebugFormat("Caught a Win32Exception error (Message: {0}). Probably tried to read an incorrect command. Error {1}", e.Message, Marshal.GetLastWin32Error());
                     }
                 }
                 else
                 {
                     // We have a comment, we don't execute it
                     pluginImpl_logger.DebugFormat("Command line : REMARK found. Not going to execute it.");
                 }
             }
             pluginImpl_logger.DebugFormat("Script execution end.");
        }


        /// <summary>
        /// ldap initialization and set-up
        /// </summary>
        public void LdapInitialization(SessionProperties props)
        {
            pluginImpl_logger.Debug("LDAP server initialization and set-up.");
            try
            {
                LdapServer serv = new LdapServer();
                props.AddTrackedSingle<LdapServer>(serv);
            }
            catch (Exception e)
            {
                pluginImpl_logger.ErrorFormat("Failed to create LdapServer: {0}", e);
                props.AddTrackedSingle<LdapServer>(null);
            }
        }

        /// <summary>
        /// cleans up any resource held by the plugin
        /// </summary>
        public void LdapEnd(SessionProperties props)
        {
            LdapServer serv = props.GetTrackedSingle<LdapServer>();
            if (serv != null) serv.Close();
        }
        
        public void Starting() { }

        public void Stopping() { }

    }
}
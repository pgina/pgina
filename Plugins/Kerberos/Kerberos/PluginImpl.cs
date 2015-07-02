using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

using log4net;
using pGina.Shared.Interfaces;

namespace pGina.Plugin.Kerberos
{
    public class PluginImpl : IPluginAuthentication, IPluginConfiguration
    {
        private ILog m_logger = LogManager.GetLogger("Kerberos");

        #region Init-plugin
        public static Guid PluginUuid
        {
            get { return new Guid("{16E22B15-4116-4FA4-9BB2-57D54BF61A43}"); }
        }

        public PluginImpl()
        {
            using (Process me = Process.GetCurrentProcess())
            {
                m_logger.DebugFormat("Plugin initialized on {0} in PID: {1} Session: {2}", Environment.MachineName, me.Id, me.SessionId);
            }
        }

        public string Name
        {
            get { return "KRB5 Authentication Plugin"; }
        }

        public string Description
        {
            get { return "Authenticates all users through kerberos"; }
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
        #endregion

        public void Configure()
        {
            pGina.Plugin.Kerberos.Configuration myDialog = new pGina.Plugin.Kerberos.Configuration();
            myDialog.ShowDialog();
        }

        public void Starting() { }
        public void Stopping() { }

        /**
         * P/Invoke for the unmanaged function that will deal with the actual athentication of a user through Kerberos
         * */
        [DllImport("authdll.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern int auth_userx86(string Username, string Password, string Domain, string Ticket);

        [DllImport("authdllx64.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern int auth_userx64(string Username, string Password, string Domain, string Ticket);

        public static int auth_user(string Username, string Password, string Domain, string Ticket)
        {
            return IntPtr.Size == 8 /* 64bit */ ? auth_userx64(Username, Password, Domain, Ticket) : auth_userx86(Username, Password, Domain, Ticket);
        }

        public pGina.Shared.Types.BooleanResult AuthenticateUser(pGina.Shared.Types.SessionProperties properties)
        {
            pGina.Shared.Types.UserInformation userInfo = properties.GetTrackedSingle<pGina.Shared.Types.UserInformation>();

            // Get the Kerberos Realm we are authenticating against from the registry
            string krbRealm = Settings.Store.Realm;
            //m_logger.InfoFormat("Kerberos Target Realm: {0}", krbRealm);

            /**
             * Call unmanaged DLL that will deal with Microsofts AcquireCredentialHandle() and InitializeSecurityContext() calls after creating a new SEC_WIN_AUTH_IDENTITY structure
             * from the supplied user name, password, and domain.  The return result will indicate either success or various kerberos error messages.
             * */
            int r = auth_user(userInfo.Username, userInfo.Password, krbRealm, "krbtgt/" + krbRealm.ToUpper());
            switch (r)
            {
                /*
                 * The SPN kerberos target service could not be reached.  Format should be <service-name>/REALM where the service is usually krbtgt (kerberos ticket granting ticket) followed by
                 * the realm you are targeting (all capitals) such as MYREALM.UTAH.EDU
                 *
                 * ex: krbtgt/MYREALM.UTAH.EDU
                 * */
                case -2146893039:
                    return new pGina.Shared.Types.BooleanResult() { Success = false, Message = "Failed to contact authenticating kerberos authority." };
                /*
                 * The user name and/or password supplied at login through pGina does not match in the kerberos realm.
                 * */
                case -2146893044:
                    return new pGina.Shared.Types.BooleanResult() { Success = false, Message = "Failed due to bad password and/or user name." };
                /*
                 * The SPN for your kerberos target was incorrect. Format should be <service-name>/REALM where the service is usually krbtgt (kerberos ticket granting ticket) followed by
                 * the realm you are targeting (all capitals) such as MYREALM.UTAH.EDU
                 *
                 * ex: krbtgt/MYREALM.UTAH.EDU
                 * */
                case -2146893053:
                    return new pGina.Shared.Types.BooleanResult() { Success = false, Message = "Failed due to bad kerberos Security Principal Name." };
                /*
                 * Success
                 * */
                case 0:
                    return new pGina.Shared.Types.BooleanResult() { Success = true, Message = "Success" };
                default:
                    return new pGina.Shared.Types.BooleanResult() { Success = false, Message = "Failed to authenticate due to unknown error." + r };
            }
        }
    }
}

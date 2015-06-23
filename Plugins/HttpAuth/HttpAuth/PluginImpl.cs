using System;
using log4net;

using pGina.Shared.Interfaces;
using pGina.Shared.Types;
using System.DirectoryServices.AccountManagement;

namespace pGina.Plugin.HttpAuth
{
    public class PluginImpl : IPluginAuthentication, IPluginAuthorization, IPluginAuthenticationGateway
    {
        public BooleanResult AuthenticateUser(SessionProperties properties)
        {
            // this method shall say if our credentials are valid
            UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();

            return HttpAccessor.getResponse(userInfo.Username, userInfo.Password);
        }

        public BooleanResult AuthorizeUser(SessionProperties properties)
        {
            // this method shall say if we are allowed to login
            // so just look at userinfo if we are ... :)
            UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
            UInfo uinfo;

            try
            {
                HttpAccessor.getUserInfo(userInfo.Username, userInfo.Password, out uinfo);
                if (uinfo.whyCannotLogin.Length == 0)
                {
                    return new BooleanResult() { Success = true };
                }
                else
                {
                    return new BooleanResult() { 
                        Success = false, Message = uinfo.whyCannotLogin
                    };
                }
            }
            catch (Exception e)
            {
                return new BooleanResult() { Success = false, Message = e.Message };
            }
        }

        public BooleanResult AuthenticatedUserGateway(SessionProperties properties)
        {
            // this method shall perform some other tasks ...

            UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
            UInfo uinfo;

            try
            {
                HttpAccessor.getUserInfo(userInfo.Username, userInfo.Password, out uinfo);
            }
            catch (Exception e)
            {
                s_logger.ErrorFormat("getUserInfo failed: {0}. Next processing aborted :(", e.Message);
                return new BooleanResult() { Success = false, Message = e.Message };
            }

            PrincipalContext ctx = new PrincipalContext(ContextType.Machine);
            GroupPrincipal grp;

            //  like group m-ship ...
            foreach (string g in uinfo.groups)
            {
                grp = GroupPrincipal.FindByIdentity(ctx, IdentityType.Name, g);
                if (grp != null)
                {
                    userInfo.Groups.Add(new GroupInformation { Name = g, SID = grp.Sid });
                }
            }

            // and what else ??? :)
            return new BooleanResult() { Success = true };
        }

        private static ILog s_logger = LogManager.GetLogger("HttpAuth");

        public string Description
        {
            get { return "Uses http(s) request to obtain user info"; }
        }

        public string Name
        {
            get { return "HttpAuth"; }
        }

        public static readonly Guid PluginUuid = new Guid("{C4FF794F-5843-4BEF-90BA-B5E4E488C662}");
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

        public void Starting() { }

        public void Stopping() { }
    }
}

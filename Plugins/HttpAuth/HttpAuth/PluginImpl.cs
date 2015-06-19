using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using pGina.Shared.Interfaces;
using pGina.Shared.Types;

namespace pGina.Plugin.HttpAuth
{
    public class PluginImpl : pGina.Shared.Interfaces.IPluginAuthentication
    {

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

        public BooleanResult AuthenticateUser(SessionProperties properties)
        {
            UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();

            return HttpAccessor.getResponse(userInfo.Username, userInfo.Password);
        }
    }
}

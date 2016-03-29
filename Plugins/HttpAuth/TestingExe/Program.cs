using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using pGina.Shared.Types;
using pGina.Plugin.HttpAuth;


namespace TestingExe
{
    class Program
    {
        static void Main(string[] args)
        {
            SessionProperties properties = new SessionProperties(new Guid("12345678-1234-1234-1234-123412341234"));
            UserInformation userInfo = new UserInformation();
            userInfo.Username = "gandalf";
            userInfo.Email = "gandalf@shire.nz";
            userInfo.Fullname = "Gandalf The Gray";
            userInfo.LoginScript = "net use x: \\lserver\bakasracky";
            userInfo.Password = "secret";
            properties.AddTrackedSingle<UserInformation>(userInfo);

            PluginImpl plugin = new PluginImpl();

            var authResult = plugin.AuthenticateUser(properties);
            Debug.Assert(authResult.Success == true, "auth should succeed!");

            var gatewayResult = plugin.AuthenticatedUserGateway(properties);
            Debug.Assert(authResult.Success == true, "gateway should succeed!");

            System.Console.Write("DONE");
        }
    }
}

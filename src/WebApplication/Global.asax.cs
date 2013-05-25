using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using ServiceStack.ServiceClient.Web;

namespace WebApplication
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            //AutonomousServiceBus.Processor.start();

            var appHost = new AutonomousServiceBus.AutonomousService.AppHost();
            appHost.Init();
            //var c = new JsonServiceClient("http://localhost:26708");
            //var eventType = new AutonomousServiceBus.ServiceContracts.EventType(200, "numbers");
            //c.Post(eventType);
            //AutonomousServiceBus.Processor.startSelfPing("http://localhost:26708", 200, "numbers");
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}
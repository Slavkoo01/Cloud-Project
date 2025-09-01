using Contracts;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace StackOverflowService
{
    public class WebRole : RoleEntryPoint
    {
        private ServiceHost serviceHost;
        private string internalEndpointName = "health-monitoring";

        public override bool OnStart()
        {
            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.
            RoleInstanceEndpoint inputEndPoint = RoleEnvironment.
            CurrentRoleInstance.InstanceEndpoints[internalEndpointName];
            string endpoint = String.Format("net.tcp://{0}/{1}", inputEndPoint.IPEndpoint,
            internalEndpointName);

            serviceHost = new ServiceHost(typeof(StackOverflowServiceServerProvider));
            NetTcpBinding binding = new NetTcpBinding();
            serviceHost.AddServiceEndpoint(typeof(IServiceHealthCheck), binding, endpoint);
            serviceHost.Open(); //----

            return base.OnStart();
        }
    }
}

using Contracts;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService
{
    public class NotificationServiceServer
    {
        private ServiceHost serviceHost;
        // dodati endpoint sa ovim imenom u ServiceDefinition
        private string internalEndpointName = "health-monitoring";
        public NotificationServiceServer() // ! 
        {
            RoleInstanceEndpoint inputEndPoint = RoleEnvironment.
            CurrentRoleInstance.InstanceEndpoints[internalEndpointName];
            string endpoint = String.Format("net.tcp://{0}/{1}", inputEndPoint.IPEndpoint,
            internalEndpointName);
            serviceHost = new ServiceHost(typeof(NotificationServiceServerProvider));
            NetTcpBinding binding = new NetTcpBinding();
            serviceHost.AddServiceEndpoint(typeof(INotificationService), binding, endpoint);
        }
        public void Open()
        {
            try
            {

                serviceHost.Open();
                Trace.TraceInformation(String.Format("Host for {0} endpoint type opened successfully at  {1} ", internalEndpointName, DateTime.Now));

            }
            catch (Exception e)
            {
                Trace.TraceInformation("Host open error for {0} endpoint type. Error message is: {1}. ", internalEndpointName, e.Message);
            }
        }
        public void Close()
        {
            try
            {

                serviceHost.Close();
                Trace.TraceInformation(String.Format("Host for {0} endpoint type closed successfully at { 1}", internalEndpointName, DateTime.Now));
            }
            catch (Exception e)
            {
                Trace.TraceInformation("Host close error for {0} endpoint type. Error message is: { 1}. ", internalEndpointName, e.Message);
            }
        }
    }
}

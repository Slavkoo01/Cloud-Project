using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.Timers;
using Contracts;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace HealthMonitoringService
{
    public class HealthMonitoringServer
    {
        private ServiceHost serviceHost;
        private string externalEndpointName = "HMonitor"; // endpoint za admin konzolu
        private string notificationInternalEndpointName = "health-monitoring"; // endpoint NotificationService
        private Timer timer;

        public HealthMonitoringServer()
        {
            RoleInstanceEndpoint inputEndPoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints[externalEndpointName];
            string endpoint = String.Format("net.tcp://{0}/{1}", inputEndPoint.IPEndpoint, externalEndpointName);
            serviceHost = new ServiceHost(typeof(HealthMonitoringServerProvider));
            NetTcpBinding binding = new NetTcpBinding();
            serviceHost.AddServiceEndpoint(typeof(IHealthMonitoring), binding, endpoint);

            timer = new Timer(4000);
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            timer.Start();
        }

        private void Timer_Elapsed(object sender = null, ElapsedEventArgs e = null)
        {
            try
            {
                // Prolazi kroz sve instance NotificationService
                foreach (var instance in RoleEnvironment.Roles["NotificationService"].Instances)
                {
                    var endpoint = instance.InstanceEndpoints[notificationInternalEndpointName];
                    string address = String.Format("net.tcp://{0}/{1}", endpoint.IPEndpoint, notificationInternalEndpointName);
                    try
                    {
                        var factory = new ChannelFactory<INotificationService>(new NetTcpBinding(), new EndpointAddress(address));
                        var proxy = factory.CreateChannel();

                        bool isAlive = proxy.HealthCheck();
                        if (isAlive)
                            Trace.TraceInformation($"NotificationService instance {instance.Id} OK.");
                        else
                            Trace.TraceWarning($"NotificationService instance {instance.Id} NOT OK!");
                    }
                    catch (Exception exInstance)
                    {
                        Trace.TraceError($"Error connecting to NotificationService instance {instance.Id}: {exInstance.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"HealthMonitoringServer error: {ex.Message}");
            }
        }

        public void Open()
        {
            try
            {
                serviceHost.Open();
                Trace.TraceInformation($"Host for {externalEndpointName} endpoint opened successfully at {DateTime.Now}");
            }
            catch (Exception e)
            {
                Trace.TraceError($"Host open error for {externalEndpointName}. Error: {e.Message}");
            }
        }

        public void Close()
        {
            try
            {
                serviceHost.Close();
                Trace.TraceInformation($"Host for {externalEndpointName} endpoint closed successfully at {DateTime.Now}");
            }
            catch (Exception e)
            {
                Trace.TraceError($"Host close error for {externalEndpointName}. Error: {e.Message}");
            }
        }
    }
}

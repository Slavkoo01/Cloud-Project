using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Timers;
using Contracts;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.Queue;
using ServiceDataRepo.Entities;
using ServiceDataRepo.Helpers;
using ServiceDataRepo.Repositories;

namespace HealthMonitoringService
{
    public class HealthMonitoringServer
    {
        private ServiceHost serviceHost;
        private string externalEndpointName = "HMonitor"; // endpoint za admin konzolu
        private string internalEndpointName = "health-monitoring"; // endpoint NotificationService i StackOverFlowService
        private Timer timer;

        public HealthMonitoringServer()
        {
            RoleInstanceEndpoint inputEndPoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints[externalEndpointName];
            string endpoint = String.Format("net.tcp://{0}/{1}", inputEndPoint.IPEndpoint, externalEndpointName);
            serviceHost = new ServiceHost(typeof(HealthMonitoringServerProvider));
            NetTcpBinding binding = new NetTcpBinding();
            serviceHost.AddServiceEndpoint(typeof(IHealthMonitoring), binding, endpoint);

            timer = new Timer(30000); //prepraviti na 4000
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            timer.Start();
        }

        private void Timer_Elapsed(object sender = null, ElapsedEventArgs e = null)
        {
            try
            {
                HealthCheck("NotificationService");
                HealthCheck("StackOverflowService");
            }
            catch (Exception ex)
            {
                Trace.TraceError($"HealthMonitoringServer error: {ex.Message}");
            }
        }

        private void HealthCheck(string serviceName)
        {
            foreach (var instance in RoleEnvironment.Roles[serviceName].Instances)
            {
                var endpoint = instance.InstanceEndpoints[internalEndpointName];
                string address = String.Format("net.tcp://{0}/{1}", endpoint.IPEndpoint, internalEndpointName);
                try
                {
                    var factory = new ChannelFactory<IServiceHealthCheck>(new NetTcpBinding(), new EndpointAddress(address));
                    var proxy = factory.CreateChannel();
                    proxy.HealthCheck();

                    Trace.TraceInformation($"{serviceName} instance {instance.Id} OK.");
                    HealthCheckLog(serviceName, "OK");
                }
                catch (EndpointNotFoundException ex)
                {
                    HealthCheckLog(serviceName, "NOT_OK");
                    FailedHealthCheck(serviceName);
                    Trace.TraceWarning($"{serviceName} instance {instance.Id} NOT OK! {ex.Message}");
                }
                catch (Exception exInstance)
                {
                    Trace.TraceError($"Error connecting to {serviceName} instance {instance.Id}: {exInstance.Message}");
                }
            }
        }

        private void FailedHealthCheck(string serviceName)
        {
            // alert mejlovi u red
            AlertEmailDataRepo aedr = new AlertEmailDataRepo();
            List<AlertEmailEntity> alertEmails = aedr.RetrieveAllAlertEmails().ToList();
            CloudQueue queue = QueueHelper.GetQueueReference("admin-notifications-queue");

            foreach (AlertEmailEntity email in alertEmails)
            {
                string subject = $"Failed Health Check - {serviceName}";
                string body = $"Servis: {serviceName}\nStatus: NOT_OK\nVreme: {DateTime.UtcNow}";
                string messageText = $"HEALTHCHECK;Subject:{subject};To:{email.Email};Body:{body}";

                queue.AddMessage(new CloudQueueMessage(messageText), null, TimeSpan.FromMilliseconds(30));
            }
            //da li ovdje ili u notification servis praviti notificationLog
        }

        private void HealthCheckLog(string serviceName, string status)
        {
            HealthCheckDataRepository hctr = new HealthCheckDataRepository();
            hctr.AddHealthCheckEntity(new HealthCheckEntity(serviceName, status));
            var fromList = hctr.RetrieveAllNotificationkServiceHealthCheckEntities();
            /*
            foreach (var from in fromList)
            {
                Trace.TraceInformation($"HealthCheckEntity: {from.PartitionKey}, {from.RowKey}, {from.Status}, {from.CheckedAt}");
            }
            */
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

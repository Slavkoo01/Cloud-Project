using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.ServiceModel;
using System.Timers;
using Contracts;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.Queue;
using ServiceDataRepo.Entities;
using ServiceDataRepo.Helpers;
using ServiceDataRepo.Repositories;
using static System.Net.WebRequestMethods;

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

        /*
        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var role in RoleEnvironment.Roles)
            {
                Trace.WriteLine($"Role name: {role.Key}");
                foreach (var instance in role.Value.Instances)
                {
                    Trace.WriteLine($"  Instance Id: {instance.Id}");
                    foreach (var endpoint in instance.InstanceEndpoints)
                    {
                        Trace.WriteLine($"    Endpoint: {endpoint.Key} = {endpoint.Value.IPEndpoint}");
                    }
                }
            }

            try
            {
                // Ovdje dodati i za dr servis healthCheck 
                // defff srediti kod

                // Prolazi kroz sve instance NotificationService
                foreach (var instance in RoleEnvironment.Roles["NotificationService"].Instances)
                {
                    var endpoint = instance.InstanceEndpoints[notificationInternalEndpointName];
                    string address = String.Format("net.tcp://{0}/{1}", endpoint.IPEndpoint, notificationInternalEndpointName);
                    try
                    {
                        // treba insertovati DateTime, Status, Naziv_Servisa
                        // 1 instanca worker health check role notification service, 2. health check treba da salje i pita, 2 threada, health service treba da samo vadi podatke iz te tabele i da ih pakuje na front
                        var factory = new ChannelFactory<INotificationService>(new NetTcpBinding(), new EndpointAddress(address));
                        var proxy = factory.CreateChannel();

                        bool isAlive = proxy.HealthCheck();
                        if (isAlive)
                            Trace.TraceInformation($"NotificationService instance {instance.Id} OK.");
                        else
                        {
                            //too many emails for now
                            //FailedHealthCheck("NotificationService");
                            Trace.TraceWarning($"NotificationService instance {instance.Id} NOT OK!");
                        }
                        HealthCheckLog("NotificationService", isAlive ? "OK" : "NOT_OK");

                    }
                    catch (Exception exInstance)
                    {
                        Trace.TraceError($"Error connecting to NotificationService instance {instance.Id}: {exInstance.Message}");
                        HealthCheckLog("NotificationService", "NOT_OK");

                    }

                }


                try
                {

                    var instance = RoleEnvironment.Roles["StackOverflowService"].Instances[4];
                    var endpoint = instance.InstanceEndpoints["Endpoint1"];
                    string address = $"http://{endpoint.IPEndpoint}/api/health-monitoring";

                    using (var client = new HttpClient())
                    {
                        var response = await client.GetAsync(address);
                        if (response.IsSuccessStatusCode)
                        {
                            Trace.TraceInformation("StackOverflowService OK.");
                            HealthCheckLog("StackOverflowService", "OK");
                        }
                        else
                        {
                            Trace.TraceWarning("StackOverflowService NOT OK!");
                            HealthCheckLog("StackOverflowService", "NOT_OK");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Error connecting to StackOverflowService: {ex.Message}");
                    HealthCheckLog("StackOverflowService", "NOT_OK");
                }


            }
            catch (Exception ex)
            {
                Trace.TraceError($"HealthMonitoringServer error: {ex.Message}");
            }
        }
        */
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

                        HealthCheckLog("NotificationService", isAlive ? "OK" : "NOT_OK");

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

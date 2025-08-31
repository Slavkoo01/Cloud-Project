using Contracts;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.Queue;
using ServiceDataRepo.Entities;
using ServiceDataRepo.Helpers;
using ServiceDataRepo.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

namespace HealthMonitoringService
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private HealthMonitoringServer hms;
        private HealthCheckDataRepository healthRepo;
        private AlertEmailDataRepo alertEmailRepo;
        private HttpClient httpClient; // koristim ga da bi napravio HTTP GET zahtev ka health check endpoint-u StackOverflow servisa
        // ako ne treba http, moze da se ukloni i da jednostavno vrati "OK" iz CheckStackOverflowService metode

        public override void Run()
        {
            Trace.TraceInformation("HealthMonitoringService is running");
            try
            {
                // Pokreni i health checking i WCF server
                Task.Run(() => this.RunAsync(this.cancellationTokenSource.Token));
                Task.Run(() => this.HealthCheckLoop(this.cancellationTokenSource.Token));

                // Èekaj da se završi
                this.runCompleteEvent.WaitOne();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit = 12;

            bool result = base.OnStart();

            // Inicijalizuj komponente
            hms = new HealthMonitoringServer();
            hms.Open();

            healthRepo = new HealthCheckDataRepository();
            alertEmailRepo = new AlertEmailDataRepo();
            httpClient = new HttpClient();

            Trace.TraceInformation("HealthMonitoringService has been started");
            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("HealthMonitoringService is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            // Cleanup
            httpClient?.Dispose();
            hms?.Close();

            base.OnStop();
            Trace.TraceInformation("HealthMonitoringService has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // Originalna logika za WCF server
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("WCF Server Working");
                await Task.Delay(10000, cancellationToken); // 10 sekundi
            }
        }

        private async Task HealthCheckLoop(CancellationToken cancellationToken)
        {
            // Nova logika za health checking
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await PerformHealthChecks();
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Error during health checks: {ex.Message}");
                }

                await Task.Delay(30000, cancellationToken); // Proverava svakih 30 sekundi
            }
        }

        private async Task PerformHealthChecks()
        {
            try
            {
                // Health check za StackOverflowService
                string stackOverflowStatus = await CheckStackOverflowService();
                var stackOverflowEntity = new HealthCheckEntity("StackOverflowService", stackOverflowStatus);
                healthRepo.AddHealthCheckEntity(stackOverflowEntity);

                Trace.TraceInformation($"StackOverflowService health check: {stackOverflowStatus}");

                // Ako StackOverflowService nije OK, pošalji alarm emailove
                if (stackOverflowStatus == "NOT_OK")
                {
                    await SendAlarmEmails("StackOverflowService");
                }

                // Health check za NotificationService - možeš dodati pravi endpoint ili ostaviti kao OK
                string notificationStatus = await CheckNotificationService();
                var notificationEntity = new HealthCheckEntity("NotificationService", notificationStatus);
                healthRepo.AddHealthCheckEntity(notificationEntity);

                Trace.TraceInformation($"NotificationService health check: {notificationStatus}");

                // Ako NotificationService nije OK, pošalji alarm emailove
                if (notificationStatus == "NOT_OK")
                {
                    await SendAlarmEmails("NotificationService");
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error during health checks: {ex.Message}");
            }
        }

        private async Task<string> CheckStackOverflowService()
        {
            try
            {
                // Prilagodi URL na osnovu stvarne konfiguracije
                var response = await httpClient.GetAsync("http://localhost:63891/api/HealthCheck");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return content.Trim().ToUpper() == "OK" ? "OK" : "NOT_OK";
                }
                return "NOT_OK";
            }
            catch (Exception ex)
            {
                Trace.TraceError($"StackOverflow health check failed: {ex.Message}");
                return "NOT_OK";
            }
        }

        private async Task<string> CheckNotificationService()
        {
            try
            {
                // Za sada vraæaj OK, možeš dodati pravi health check poziv
                // ili implementirati WCF poziv ka NotificationService health check metodi
                return "OK";
            }
            catch (Exception ex)
            {
                Trace.TraceError($"NotificationService health check failed: {ex.Message}");
                return "NOT_OK";
            }
        }

        private async Task SendAlarmEmails(string serviceName)
        {
            try
            {
                var alertEmails = alertEmailRepo.RetrieveAllAlertEmails().ToList();

                CloudQueue queue = QueueHelper.GetQueueReference("admin-notifications-queue");

                foreach (var alertEmail in alertEmails)
                {
                    string subject = $"ALERT: {serviceName} is DOWN";
                    string body = $"<h2>Service Alert</h2><p>The service <strong>{serviceName}</strong> is currently not responding.</p><p>Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>";

                    string message = $"HEALTHCHECK;Subject:{subject};Email:{alertEmail.Email};Body:{body}";

                    var queueMessage = new CloudQueueMessage(message);
                    await queue.AddMessageAsync(queueMessage);
                }

                Trace.TraceInformation($"Alarm emails queued for {serviceName} failure");
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error sending alarm emails: {ex.Message}");
            }
        }
    }
}

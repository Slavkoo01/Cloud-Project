using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.Queue;
using ServiceDataRepo.Entities;
using ServiceDataRepo.Helpers;
using ServiceDataRepo.Repositories;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HealthCheckService
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private HealthCheckDataRepository healthRepo;
        private AlertEmailDataRepo alertEmailRepo;
        private HttpClient httpClient;

        public override void Run()
        {
            Trace.TraceInformation("HealthCheckService is running");
            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            bool result = base.OnStart();

            healthRepo = new HealthCheckDataRepository();
            alertEmailRepo = new AlertEmailDataRepo();
            httpClient = new HttpClient();

            Trace.TraceInformation("HealthCheckService has been started");
            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("HealthCheckService is stopping");
            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            httpClient?.Dispose();

            base.OnStop();
            Trace.TraceInformation("HealthCheckService has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await PerformHealthChecks();
                await Task.Delay(30000); // Proverava svakih 30 sekundi
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

                // Health check za NotificationService
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
                // Poziv ka NotificationService WCF servisu za health check
                // Ovo bi trebalo da bude implementirano kroz WCF proxy
                // Za sada vraćam OK, ali trebalo bi implementirati pravi health check
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
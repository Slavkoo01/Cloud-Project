using Contracts;
using Microsoft.WindowsAzure.Storage.Queue;
using ServiceDataRepo.Entities;
using ServiceDataRepo.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService
{
    public class NotificationServiceServerProvider : INotificationService
    {
        // Procesira notifikaciju baziranu na AnswerId (opciono, može se kasnije implementirati).
        public void ProcessNotification(Guid answerId)
        {
            // Ovo može da se koristi ako želiš da obaveštavaš korisnike kad odgovor promeni status.
            // Za sada ostavljamo prazno ili možemo napisati log ako je potrebno.
            Trace.TraceInformation($"ProcessNotification called for AnswerId: {answerId}");
        }

        // Dodaje poruke u Azure Queue za slanje emailova putem WorkerRole-a.
        public void SendEmails(IEnumerable<string> recipients, string subject, string body)
        {
            if (recipients == null || !recipients.Any())
            {
                Trace.TraceWarning("SendEmails called with no recipients.");
                return;
            }

            try
            {
                CloudQueue queue = QueueHelper.GetQueueReference("admin-notifications-queue");

                foreach (var email in recipients)
                {
                    if (!string.IsNullOrEmpty(email))
                    {
                        string message = $"HEALTHCHECK;Subject:{subject};Email:{email};Body:{body}";
                        queue.AddMessage(new CloudQueueMessage(message));
                    }
                }

                Trace.TraceInformation($"SendEmails queued {recipients.Count()} messages successfully.");
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error in SendEmails: {ex.Message}");
            }
        }

        // Loguje notifikaciju u tabelu (opciono, može se kasnije dopuniti).
        public void LogNotification(NotificationLogEntity notificationLog)
        {
            if (notificationLog == null)
            {
                Trace.TraceWarning("LogNotification called with null entity.");
                return;
            }

            Trace.TraceInformation($"LogNotification called for: {notificationLog.RowKey}");
            // Ovde bi išla implementacija za Azure Table storage ako želimo čuvanje logova.
        }

        // Health check za WCF servis.
        public bool HealthCheck()
        {
            return true;
        }
    }

}

using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.Queue;
using ServiceDataRepo.Entities;
using ServiceDataRepo.Helpers;
using ServiceDataRepo.Repositories;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationService
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private NotificationServiceServer server = new NotificationServiceServer();
        private NotificationEmailSentRepo notificationEmailRepo;

        public override void Run()
        {
            CloudQueue adminQueue = QueueHelper.GetQueueReference("admin-notifications-queue");
            CloudQueue notificationQueue = QueueHelper.GetQueueReference("notifications"); // Nova queue za best answer notifications

            Trace.TraceInformation("NotificationService is running");

            while (true)
            {
                try
                {
                    // Obradi admin poruke (health check alarmi)
                    ProcessAdminMessages(adminQueue);

                    // Obradi notification poruke (najbolji odgovori)
                    ProcessNotificationMessages(notificationQueue);
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Error in NotificationService: {ex.Message}");
                }

                Thread.Sleep(5000);
                Trace.TraceInformation("Working", "Information");
            }
        }

        public override bool OnStart()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit = 12;

            bool result = base.OnStart();

            server.Open();
            notificationEmailRepo = new NotificationEmailSentRepo();

            Trace.TraceInformation("NotificationService has been started");
            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("NotificationService is stopping");
            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();
            base.OnStop();
            server.Close();
            Trace.TraceInformation("NotificationService has stopped");
        }

        private void ProcessAdminMessages(CloudQueue adminQueue)
        {
            CloudQueueMessage message = adminQueue.GetMessage();
            if (message != null)
            {
                Trace.TraceInformation($"Admin message received: {message.AsString}");

                if (message.DequeueCount > 3)
                {
                    adminQueue.DeleteMessage(message);
                    return;
                }

                try
                {
                    SendAlertEmails(message.AsString).GetAwaiter().GetResult();
                    adminQueue.DeleteMessage(message);
                    Trace.TraceInformation($"Admin message processed: {message.AsString}");
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Error processing admin message: {ex.Message}");
                }
            }
        }

        private void ProcessNotificationMessages(CloudQueue notificationQueue)
        {
            CloudQueueMessage message = notificationQueue.GetMessage();
            if (message != null)
            {
                Trace.TraceInformation($"Notification message received: {message.AsString}");

                if (message.DequeueCount > 3)
                {
                    notificationQueue.DeleteMessage(message);
                    return;
                }

                try
                {
                    // Poruka sadrži ID odgovora koji je oznaèen kao najbolji
                    if (Guid.TryParse(message.AsString, out Guid answerId))
                    {
                        ProcessBestAnswerNotification(answerId).GetAwaiter().GetResult();
                        notificationQueue.DeleteMessage(message);
                        Trace.TraceInformation($"Best answer notification processed for answer: {answerId}");
                    }
                    else
                    {
                        Trace.TraceWarning($"Invalid answer ID in notification message: {message.AsString}");
                        notificationQueue.DeleteMessage(message);
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Error processing notification message: {ex.Message}");
                }
            }
        }

        private async Task ProcessBestAnswerNotification(Guid answerId)
        {
            try
            {
                // Ovde bi trebalo da dohvatiš podatke o odgovoru, pitanju i korisnicima
                // koji su odgovorili na to pitanje iz StackOverflowService
                // Za sada simuliram podatke

                var recipients = GetQuestionParticipants(answerId); // Ova metoda treba da se implementira

                if (recipients != null && recipients.Any())
                {
                    string subject = "Best Answer Selected";
                    string body = $"<h2>Best Answer Notification</h2><p>A best answer has been selected for a question you participated in.</p><p>Answer ID: {answerId}</p><p>Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>";

                    int emailCount = 0;
                    foreach (var recipient in recipients)
                    {
                        await SendNotificationEmail(recipient, subject, body);
                        emailCount++;
                    }

                    // Zabeleži u tabelu broj poslanih emailova
                    var notificationEntity = new NotificationEmailSentEntity(answerId, emailCount);
                    notificationEmailRepo.AddNotificationEmailSent(notificationEntity);

                    Trace.TraceInformation($"Best answer notification emails sent: {emailCount} for answer {answerId}");
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error processing best answer notification: {ex.Message}");
                throw;
            }
        }

        private string[] GetQuestionParticipants(Guid answerId)
        {
            // Ova metoda treba da pozove StackOverflowService da dobije 
            // sve korisnike koji su odgovorili na pitanje koje sadrži dati answerId
            // Za sada vraæam test emailove
            return new[] { "test1@example.com", "test2@example.com" };
        }

        private async Task SendAlertEmails(string message)
        {
            var smtpHost = "smtp.gmail.com";
            var smtpPort = 587;
            var smtpUsername = "projekat.drs6@gmail.com";
            var smtpPassword = "mlfb ayje vbez nnch";

            string subject = String.Empty;
            string alertEmail = String.Empty;
            string body = String.Empty;
            List<string> emails = new List<string>();

            if (message.StartsWith("HEALTHCHECK;"))
            {
                message = message.Substring("HEALTHCHECK;".Length);
                var parts = message.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                subject = parts[0].Split(new char[] { ':' }, 2)[1].Trim();
                alertEmail = parts[1].Split(new char[] { ':' }, 2)[1].Trim();
                body = parts[2].Split(new char[] { ':' }, 2)[1].Trim();
                emails.Add(alertEmail);
            }
            else
            {
                // Onda je answerID i trazimo mejlove na koje saljemo..
               
                AnswerTableRepository answerRepo = new AnswerTableRepository();
                QuestionTableRepository questionRepo = new QuestionTableRepository();
                UserTableRepository userRepo = new UserTableRepository();
                
                var allAnswers = answerRepo.GetAll();
                var answer = allAnswers.FirstOrDefault(a => a.RowKey == message); //message=answerId

                if (answer != null)
                {
                    var answersToQuestion = answerRepo.GetAll(answer.QuestionId).ToList();
                    
                    foreach (var ans in answersToQuestion)
                    {
                        var user = userRepo.GetById("User", ans.Username);
                        if (user != null)
                        {
                            if (!string.IsNullOrEmpty(user.Email) && !emails.Contains(user.Email))
                            {
                                emails.Add(user.Email);
                            }
                        }
                    }

                    var question = questionRepo.GetById("Question", answer.QuestionId); // ovo samo da ime nadjemo
                    subject = "Answer Accepted!";
                    body = $"Your answer for question '{question.Title}' has been accepted.";
                }
            }

            using (var smtp = new SmtpClient(smtpHost, smtpPort))
            {
                smtp.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                smtp.EnableSsl = true;
<<<<<<< Updated upstream

                foreach (var emailAddress in emails)
                {
                    var email = new MailMessage(smtpUsername, emailAddress, subject, body);
                    email.IsBodyHtml = true;

                    await smtp.SendMailAsync(email);
                }
=======
                var email = new MailMessage(smtpUsername, alertEmail, subject, body);
                email.IsBodyHtml = true;
                await smtp.SendMailAsync(email);
            }
        }

        private async Task SendNotificationEmail(string recipient, string subject, string body)
        {
            var smtpHost = "smtp.gmail.com";
            var smtpPort = 587;
            var smtpUsername = "projekat.drs6@gmail.com";
            var smtpPassword = "mlfb ayje vbez nnch";

            using (var smtp = new SmtpClient(smtpHost, smtpPort))
            {
                smtp.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                smtp.EnableSsl = true;
                var email = new MailMessage(smtpUsername, recipient, subject, body);
                email.IsBodyHtml = true;
                await smtp.SendMailAsync(email);
>>>>>>> Stashed changes
            }
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }
    }
}
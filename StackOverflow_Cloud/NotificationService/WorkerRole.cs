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
        public override void Run()
        {
            CloudQueue queue = QueueHelper.GetQueueReference("admin-notifications-queue");

            Trace.TraceInformation("NotificationService is running");

            while (true)
            {
                CloudQueueMessage message = queue.GetMessage();
                if (message == null)
                {
                    Trace.TraceInformation("Trenutno ne postoji poruka u redu.", "Information");
                }
                else
                {
                    Trace.TraceInformation(String.Format("Poruka glasi: {0}", message.AsString), "Information");

                    if (message.DequeueCount > 3)
                    {
                        queue.DeleteMessage(message);
                    }

                    // sta raditi sa por
                    SendAlertEmails(message.AsString).GetAwaiter().GetResult();
                    //queue.DeleteMessage(message);
                    Trace.TraceInformation($"Poruka procesuirana: {message.AsString}", "Information");
                }

                Thread.Sleep(5000);
                Trace.TraceInformation("Working", "Information");
            }
        }

        public override bool OnStart()
        {
            // Use TLS 1.2 for Service Bus connections
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();
            server.Open();
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

        //srediti ovo poslijeee - myb odvojiti ovaj dio za slanje
        private async Task SendAlertEmails(string message)
        {
            // ovo mozemo u konfig
            var smtpHost = "smtp.gmail.com";
            var smtpPort = 587;
            var smtpUsername = "projekat.drs6@gmail.com";
            var smtpPassword = "mlfb ayje vbez nnch";
            //------------------------

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

                foreach (var emailAddress in emails)
                {
                    var email = new MailMessage(smtpUsername, emailAddress, subject, body);
                    email.IsBodyHtml = true;

                    await smtp.SendMailAsync(email);
                }
            }
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }
    }
}

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
            CloudQueue notificationQueue = QueueHelper.GetQueueReference("admin-notifications-queue");
            CloudQueue answerQueue = QueueHelper.GetQueueReference("accepted-answers-queue");

            Trace.TraceInformation("NotificationService is running");

            while (true)
            {
                CloudQueueMessage notificationMessage = notificationQueue.GetMessage();
                CloudQueueMessage answerMessage = answerQueue.GetMessage();

                ProcessQueueMessage(notificationQueue, notificationMessage, "admin-notifications");
                ProcessQueueMessage(answerQueue, answerMessage, "accepted-answers");

                Thread.Sleep(5000);
                Trace.TraceInformation("Working", "Information");
            }
        }
        private void ProcessQueueMessage(CloudQueue queue, CloudQueueMessage message, string queueName)
        {
            if (message == null)
            {
                Trace.TraceInformation($"Trenutno ne postoji poruka u {queueName} redu.", "Information");
            }
            else
            {
                Trace.TraceInformation($"Poruka glasi: {message.AsString}", "Information");

                if (message.DequeueCount > 3)
                {
                    queue.DeleteMessage(message);
                }
                else
                {
                    SendAlertEmails(message.AsString).GetAwaiter().GetResult();

                    queue.DeleteMessage(message); // ovo zakom ako izadje greska
                    Trace.TraceInformation($"Poruka procesuirana: {message.AsString}", "Information");
                }
            }
        }

        public override bool OnStart()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit = 12;

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

   
        // Zadržava postojeæu logiku za health check emailove
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

            if (message == null)
                return;

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
                try
                {
                    AnswerTableRepository answerRepo = new AnswerTableRepository();
                    QuestionTableRepository questionRepo = new QuestionTableRepository();
                    UserTableRepository userRepo = new UserTableRepository();


                    List<AnswerEntity> allAnswers = answerRepo.GetAll().ToList();
                    AnswerEntity answer = null;
                    foreach (AnswerEntity e in allAnswers)
                    {
                        if (e.RowKey == message)
                        {
                            answer = e;
                        }
                    }
                    //  AnswerEntity answer = allAnswers.FirstOrDefault(a => a.RowKey.Equals(message)); //message=answerId

                    if (answer != null)
                    {
                        var answersToQuestion = answerRepo.GetAll(answer.QuestionId).ToList();
                        Console.WriteLine(allAnswers.Count());

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

                        subject = $"Question {question.Title} is closed";
                        //body = $"Your answer for question has been accepted.";
                        body = $"Answer by {answer.Username} has been accepted " + '\n' + $"{answer.Text}";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving emails for answer ID {message}: {ex.Message}");
                    return; // Exit if we can't retrieve emails
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
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }
    }
}

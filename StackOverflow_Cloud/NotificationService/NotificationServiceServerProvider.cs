using Contracts;
using ServiceDataRepo.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService
{
    public class NotificationServiceServerProvider : INotificationService
    {
        public void ProcessNotification(Guid answerId)
        {
        }
        public void SendEmails(IEnumerable<string> recipients, string subject, string body)
        {
        }
        public void LogNotification(NotificationLogEntity notificationLog)
        {
        }
        
        public void HealthCheck() {}
    }

}

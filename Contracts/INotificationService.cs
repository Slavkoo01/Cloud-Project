using ServiceDataRepo.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    [ServiceContract]
    public interface INotificationService : IServiceHealthCheck
    {
        [OperationContract]
        void ProcessNotification(Guid answerId);    // NotificationService run metodaa - > 
                                                    // cita se iz reda notifications i poziva ova metoda da na osnovu answerId-a dobijemo
                                                    // pitanje i korisnike koji su odgovorili na to pitanje

        [OperationContract]
        void SendEmails(IEnumerable<string> recipients, string subject, string body);


        [OperationContract]
        void LogNotification(NotificationLogEntity notificationLog);

    }
}

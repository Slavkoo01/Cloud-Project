using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using ServiceDataRepo.Entities;

namespace Contracts
{
    [ServiceContract]
    public interface IHealthMonitoring
    {
        // list svih email adr, izmjena, dodavanje, brisanje
        [OperationContract]
        List<AlertEmailEntity> RetrieveAllAlertEmails();

        //treba li update (?)
        [OperationContract]
        void UpdateAlertEmail(string oldEmail, AlertEmailEntity email);

        [OperationContract]
        void AddAlertEmail(AlertEmailEntity email);

        [OperationContract]
        void RemoveAlertEmail(string id);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts;
using ServiceDataRepo.Entities;
using ServiceDataRepo.Repositories;

namespace HealthMonitoringService
{
    public class HealthMonitoringServerProvider : IHealthMonitoring
    {
        private readonly AlertEmailDataRepo aedr = new AlertEmailDataRepo();
        public List<AlertEmailEntity> RetrieveAllAlertEmails()
        {
            return aedr.RetrieveAllAlertEmails().ToList();
        }
        public void UpdateAlertEmail(string oldEmail, AlertEmailEntity email)
        {
            aedr.UpdateAlertEmail(oldEmail, email);
        }
        public void AddAlertEmail(AlertEmailEntity email)
        {
            aedr.AddAlertEmail(email);
        }
        public void RemoveAlertEmail(string id)
        {
            aedr.RemoveAlertEmail(id);
        }
    }
}

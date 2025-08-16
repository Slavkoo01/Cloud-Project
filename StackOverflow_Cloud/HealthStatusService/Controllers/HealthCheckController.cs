using HealthStatusService.ViewModel;
using ServiceDataRepo.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HealthStatusService.Controllers
{
        public class HealthCheckController : Controller
        {

            HealthCheckDataRepository repo = new HealthCheckDataRepository();
            // GET: HealthCheck


            public ActionResult HealthCheck()
            {
                DateTime threeHoursAgo = DateTime.UtcNow.AddHours(-3);
                var recentEntities = repo.RetrieveAllNotificationkServiceHealthCheckEntities().ToList()
                                  .Where(e => e.CheckedAt >= threeHoursAgo)
                                  .OrderBy(e => e.CheckedAt)
                                  .ToList();


            var dtoList = recentEntities.Select(e => new HealthCheckDTO
                {
                    ServiceName = e.PartitionKey,
                    Status = e.Status,
                    CheckedAt = e.CheckedAt
                }).ToList();

                var totalEntities = recentEntities.Count();
                int availableCount = recentEntities.Count(e => e.Status == "OK");
                int unavailableCount = recentEntities.Count(e => e.Status == "NOT_OK");
                double availablePercentage = totalEntities > 0 ? (availableCount / (double)totalEntities) * 100 : 0;
                double unavailablePercentage = 100 - availablePercentage;
                var viewModel = new HealthStatusViewModel
                {
                    Records = dtoList,
                    AvailablePercent = Math.Round(availablePercentage, 2),
                    UnavailablePercent = Math.Round(unavailablePercentage, 2)
                };

                return View(viewModel);
            }
        }
}
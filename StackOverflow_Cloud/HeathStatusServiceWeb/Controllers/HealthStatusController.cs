using ServiceDataRepo.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HeathStatusServiceWeb.Controllers
{
    [RoutePrefix("api/health")]
    public class HealthStatusController : ApiController
    {
        private readonly HealthCheckDataRepository healthRepo;

        public HealthStatusController()
        {
            healthRepo = new HealthCheckDataRepository();
        }

        [HttpGet]
        [Route("stackoverflow")]
        public IHttpActionResult GetStackOverflowHealthChecks()
        {
            var entities = healthRepo.RetrieveAllHealthCheckEntities()
                                     .OrderByDescending(e => e.CheckedAt)
                                     .Take(50)
                                     .ToList();

            var result = entities.Select(e => new
            {
                Service = e.PartitionKey,
                Status = e.Status,
                CheckedAt = e.CheckedAt
            });

            return Ok(result);
        }

        [HttpGet]
        [Route("notification")]
        public IHttpActionResult GetNotificationHealthChecks()
        {
            var entities = healthRepo.RetrieveAllNotificationkServiceHealthCheckEntities()
                                     .OrderByDescending(e => e.CheckedAt)
                                     .Take(50)
                                     .ToList();

            var result = entities.Select(e => new
            {
                Service = e.PartitionKey,
                Status = e.Status,
                CheckedAt = e.CheckedAt
            });

            return Ok(result);
        }

        // Nova metoda za live updating - vraća samo nove statuse nakon određenog vremena
        [HttpGet]
        [Route("stackoverflow/since")]
        public IHttpActionResult GetStackOverflowHealthChecksSince([FromUri] DateTime? lastCheckTime)
        {
            if (!lastCheckTime.HasValue)
            {
                // Ako nema poslednjeg vremena, vrati poslednje
                return GetStackOverflowHealthChecks();
            }

            var entities = healthRepo.RetrieveAllHealthCheckEntities()
                                     .Where(e => e.CheckedAt > lastCheckTime.Value)
                                     .OrderByDescending(e => e.CheckedAt)
                                     .ToList();

            var result = entities.Select(e => new
            {
                Service = e.PartitionKey,
                Status = e.Status,
                CheckedAt = e.CheckedAt
            });

            return Ok(result);
        }

        // Nova metoda za live updating - vraća samo nove statuse nakon određenog vremena
        [HttpGet]
        [Route("notification/since")]
        public IHttpActionResult GetNotificationHealthChecksSince([FromUri] DateTime? lastCheckTime)
        {
            if (!lastCheckTime.HasValue)
            {
                // Ako nema poslednjeg vremena, vrati poslednje
                return GetNotificationHealthChecks();
            }

            var entities = healthRepo.RetrieveAllNotificationkServiceHealthCheckEntities()
                                     .Where(e => e.CheckedAt > lastCheckTime.Value)
                                     .OrderByDescending(e => e.CheckedAt)
                                     .ToList();

            var result = entities.Select(e => new
            {
                Service = e.PartitionKey,
                Status = e.Status,
                CheckedAt = e.CheckedAt
            });

            return Ok(result);
        }

        // Kombinovana metoda za oba servisa
        [HttpGet]
        [Route("all")]
        public IHttpActionResult GetAllHealthChecks()
        {
            var stackOverflowEntities = healthRepo.RetrieveAllHealthCheckEntities()
                                                  .Take(25)
                                                  .ToList();

            var notificationEntities = healthRepo.RetrieveAllNotificationkServiceHealthCheckEntities()
                                                 .Take(25)
                                                 .ToList();

            var allEntities = stackOverflowEntities.Concat(notificationEntities)
                                                   .OrderByDescending(e => e.CheckedAt)
                                                   .Take(50);

            var result = allEntities.Select(e => new
            {
                Service = e.PartitionKey,
                Status = e.Status,
                CheckedAt = e.CheckedAt
            });

            return Ok(result);
        }

        // Kombinovana metoda za live updating oba servisa
        [HttpGet]
        [Route("all/since")]
        public IHttpActionResult GetAllHealthChecksSince([FromUri] DateTime? lastCheckTime)
        {
            if (!lastCheckTime.HasValue)
            {
                return GetAllHealthChecks();
            }

            var stackOverflowEntities = healthRepo.RetrieveAllHealthCheckEntities()
                                                  .Where(e => e.CheckedAt > lastCheckTime.Value)
                                                  .ToList();

            var notificationEntities = healthRepo.RetrieveAllNotificationkServiceHealthCheckEntities()
                                                 .Where(e => e.CheckedAt > lastCheckTime.Value)
                                                 .ToList();

            var allEntities = stackOverflowEntities.Concat(notificationEntities)
                                                   .OrderByDescending(e => e.CheckedAt);

            var result = allEntities.Select(e => new
            {
                Service = e.PartitionKey,
                Status = e.Status,
                CheckedAt = e.CheckedAt
            });

            return Ok(result);
        }

        // Endpoint za status summary
        [HttpGet]
        [Route("summary")]
        public IHttpActionResult GetHealthSummary()
        {
            var stackOverflowLatest = healthRepo.RetrieveAllHealthCheckEntities()
                                                .OrderByDescending(e => e.CheckedAt)
                                                .FirstOrDefault();

            var notificationLatest = healthRepo.RetrieveAllNotificationkServiceHealthCheckEntities()
                                               .OrderByDescending(e => e.CheckedAt)
                                               .FirstOrDefault();

            var result = new
            {
                StackOverflowService = new
                {
                    Status = stackOverflowLatest?.Status ?? "UNKNOWN",
                    LastChecked = stackOverflowLatest?.CheckedAt
                },
                NotificationService = new
                {
                    Status = notificationLatest?.Status ?? "UNKNOWN",
                    LastChecked = notificationLatest?.CheckedAt
                },
                OverallStatus = (stackOverflowLatest?.Status == "OK" && notificationLatest?.Status == "OK") ? "OK" : "NOT_OK"
            };

            return Ok(result);
        }
    }
}
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
    }
}

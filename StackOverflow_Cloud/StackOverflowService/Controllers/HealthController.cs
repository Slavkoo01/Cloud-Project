using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace StackOverflowService.Controllers
{
    [RoutePrefix("api/health-monitoring")]
    public class HealthMonitoringController : ApiController
    {
        [HttpGet]
        [Route("")]
        public IHttpActionResult Check()
        {
            return Ok(new { status = "OK" });
        }
    }
}

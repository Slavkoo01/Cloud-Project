using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace StackOverflowService.Controllers
{
    [RoutePrefix("api")]
    public class HealthCheckController : ApiController
    {
        [HttpGet]
        [Route("HealthCheck")]
        public IHttpActionResult HealthCheck()
        {
            try
            {
                // Ovde možeš dodati dodatne provere:
                // - Da li je baza dostupna
                // - Da li su potrebni servisi pokretani
                // - Da li su connection stringovi validni, itd.

                // Za sada vraća jednostavno OK
                return Ok("OK");
            }
            catch
            {
                return InternalServerError();
            }
        }
    }
}
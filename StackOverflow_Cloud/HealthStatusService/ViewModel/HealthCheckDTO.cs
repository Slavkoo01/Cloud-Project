using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HealthStatusService.ViewModel
{
    public class HealthCheckDTO
    {
        public string ServiceName { get; set; }
        public string Status { get; set; }
        public DateTime CheckedAt { get; set; }
    }
}
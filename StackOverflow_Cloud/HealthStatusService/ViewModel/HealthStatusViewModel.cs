using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HealthStatusService.ViewModel
{
    public class HealthStatusViewModel
    {
        public List<HealthCheckDTO> Records { get; set; }
        public double AvailablePercent { get; set; }
        public double UnavailablePercent { get; set; }
    }
}
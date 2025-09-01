using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Contracts;

namespace StackOverflowService
{
    public class StackOverflowServiceServerProvider : IServiceHealthCheck
    {
        public void HealthCheck() { }
    }
}
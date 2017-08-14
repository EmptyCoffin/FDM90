using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace FDM90.Models
{
    public class MarketingModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string MetricsUsed { get; set; }
        public string ResultMetric { get; set; }
        public string CalculationExpression { get; set; }
    }
}
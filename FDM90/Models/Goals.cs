using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FDM90.Models
{
    public class Goals
    {
        public Guid UserId { get; set; }
        public string GoalName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Targets { get; set; }
        public string Progress { get; set; }
    }
}
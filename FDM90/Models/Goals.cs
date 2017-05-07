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
        public int WeekStart { get; set; }
        public int WeekEnd { get; set; }
        public string Targets { get; set; }
        public string Progress { get; set; }
    }
}
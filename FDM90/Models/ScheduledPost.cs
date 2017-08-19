using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FDM90.Models
{
    public class ScheduledPost
    {
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public string PostText { get; set; }
        public string AttachmentPath { get; set; }
        public DateTime PostTime { get; set; }
        public string MediaChannels { get; set; }
    }
}
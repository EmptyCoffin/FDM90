using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FDM90.Models
{
    public class MediaCredentials
    {
        public Guid UserId { get; set; }
        public string UserName {get;set;}
        public string Password { get; set; }
    }
}
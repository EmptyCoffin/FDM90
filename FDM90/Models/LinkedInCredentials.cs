using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FDM90.Models
{
    public class LinkedInCredentials : MediaCredentials
    {
        public string AccessToken { get; set; }

        public DateTime ExpirationDate { get; set; }
    }
}
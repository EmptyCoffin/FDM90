using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FDM90.Models
{
    public class FacebookCredentials : MediaCredentials
    {
        public FacebookCredentials() { }

        public FacebookCredentials(Guid userId, string pageName)
        {
            this.UserId = userId;
            this.PageName = pageName;
        }

        public string PageName { get; set; }
        public string PermanentAccessToken { get; set; }

        public string FacebookData { get; set; }

    }
}
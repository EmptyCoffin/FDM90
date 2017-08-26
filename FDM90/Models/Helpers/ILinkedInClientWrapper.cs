using LinkedIn.NET.Updates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDM90.Models.Helpers
{
    public interface ILinkedInClientWrapper
    {
        string GetLoginUrl();
        LinkedInCredentials GetPermanentAccessToken(string authorizationCode);
        IEnumerable<LinkedInUpdate> GetUpdates(string accessToken, DateTime[] dates);
    }
}

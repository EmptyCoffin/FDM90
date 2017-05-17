using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDM90.Models.Helpers
{
    public interface IFacebookClientWrapper
    {
        string GetLoginUrl();
        string GetPermanentAccessToken(string shortTermToken, string pageName);
        dynamic GetData(string url, string currentData);
        dynamic GetData(string url, string currentData, object parameters);

    }
}

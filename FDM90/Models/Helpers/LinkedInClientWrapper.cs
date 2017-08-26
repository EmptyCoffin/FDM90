using FDM90.Singleton;
using LinkedIn.NET;
using LinkedIn.NET.Options;
using LinkedIn.NET.Updates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FDM90.Models.Helpers
{
    public class LinkedInClientWrapper : ILinkedInClientWrapper
    {
        private LinkedInClient _apiClient;
        public LinkedInClient ApiClient
        {
            get
            {
                if (_apiClient == null)
                {
                    _apiClient = new LinkedInClient(ConfigSingleton.LinkedInClientId, ConfigSingleton.LinkedInClientSecret);
                }

                return _apiClient;
            }
        }


        public string GetLoginUrl()
        {
            var options = new LinkedInAuthorizationOptions
            {
                RedirectUrl = "http://localhost:1900/Pages/Content/LinkedIn.aspx",
                Permissions = LinkedInPermissions.BasicProfile,
                State = Guid.NewGuid().ToString()
            };
            // Prepare authorization URL 
            return ApiClient.GetAuthorizationUrl(options);
        }

        public LinkedInCredentials GetPermanentAccessToken(string authorizationCode)
        {
            //generate longer live token
            var redirectUrl = "http://localhost:1900/Pages/Content/LinkedIn.aspx";
            var response = ApiClient.GetAccessToken(authorizationCode, redirectUrl);

            return new LinkedInCredentials() { AccessToken = response.Result.AccessToken, ExpirationDate = response.Result.Expiration };
        }

        public IEnumerable<LinkedInUpdate> GetUpdates(string accessToken, DateTime[] dates)
        {
            ApiClient.AccessToken = accessToken;
            var test = ApiClient.GetUpdates(new LinkedInGetUpdatesOptions()
            {
                Before = dates.Last(),
                After = dates.First(),
                UpdateType = LinkedInUpdateType.CompanyFollowUpdate | LinkedInUpdateType.ConnectionUpdate | LinkedInUpdateType.PostedJobUpdate | LinkedInUpdateType.StatusUpdate
            }).Result;

            return test;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;
using FDM90.Models.Helpers;
using System.Threading.Tasks;
using FDM90.Models;
using FDM90.Repository;

namespace FDM90.Handlers
{
    public class LinkedInHandler : ILinkedInHandler
    {
        private ILinkedInClientWrapper _linkedInClientWrapper;
        private IRepository<LinkedInCredentials> _linkedInRepo;
        private IReadSpecific<LinkedInCredentials> _linkedInReadRepo;
        private IUserHandler _userHandler;

        public LinkedInHandler():this(new LinkedInClientWrapper(), new LinkedInRepository(), new UserHandler())
        {

        }

        public LinkedInHandler(ILinkedInClientWrapper linkedInClientWrapper, IRepository<LinkedInCredentials> linkedInRepo, IUserHandler userHandler)
        {
            _linkedInClientWrapper = linkedInClientWrapper;
            _linkedInRepo = linkedInRepo;
            _linkedInReadRepo = (IReadSpecific<LinkedInCredentials>)linkedInRepo;
            _userHandler = userHandler;
        }

        public string MediaName
        {
            get
            {
                return "LinkedIn";
            }
        }

        public string GetLoginUrl()
        {
            return _linkedInClientWrapper.GetLoginUrl();
        }

        public Task SetAccessToken(Guid userId, string authorizationCode)
        {
            LinkedInCredentials linkedInCreds = _linkedInClientWrapper.GetPermanentAccessToken(authorizationCode);
            linkedInCreds.UserId = userId;

            _linkedInRepo.Create(linkedInCreds);

            _userHandler.UpdateUserMediaActivation(new User(linkedInCreds.UserId), MediaName);

            return Task.Factory.StartNew(() => GetMediaData(userId, DateHelper.GetDates(DateTime.Now.AddMonths(-1).Date, DateTime.Now.Date)));
        }

        public void DailyUpdate()
        {
            throw new NotImplementedException();
        }

        public IJEnumerable<JToken> GetCampaignInfo(Guid userId, DateTime[] dates)
        {
            throw new NotImplementedException();
        }

        public void GetMediaData(Guid userId, DateTime[] dates)
        {
            var userCreds = _linkedInReadRepo.ReadSpecific(userId.ToString());
            var test = _linkedInClientWrapper.GetUpdates(userCreds.AccessToken, dates);
        }

        public void PostData(Dictionary<string, string> postParameters, Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}
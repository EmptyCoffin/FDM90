using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDM90.Handlers
{
    public interface IMediaHandler
    {
        string MediaName { get; }
        IJEnumerable<JToken> GetCampaignInfo(Guid userId, DateTime[] dates);
        void GetMediaData(Guid userId, DateTime[] dates);
        void PostData(Dictionary<string, string> postParameters, Guid userId);
    }
}

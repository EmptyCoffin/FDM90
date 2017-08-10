using FDM90.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDM90.Handlers
{
    public interface ICampaignHandler
    {
        void CreateCampaign(User user, string name, string weekStart, string weekEnd, string targets);

        List<Campaign> GetUserCampaigns(Guid userId);

        Task<bool> DailyUpdate();
    }
}

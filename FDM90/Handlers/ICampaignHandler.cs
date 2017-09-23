using FDM90.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDM90.Handlers
{
    public interface ICampaignHandler
    {
        Task CreateCampaign(User user, string name, string weekStart, string weekEnd, string targets);

        List<Campaign> GetUserCampaigns(Guid userId);

        DataTable GenerateCampaignDataTable(Campaign campaign);

        Task<bool> DailyUpdate();

        void DeleteForUser(Guid userId);

        void RemoveMediaAfterDelete(Guid userId, string mediaName);

        void DeleteCampaign(Campaign deletingCampaign);
    }
}

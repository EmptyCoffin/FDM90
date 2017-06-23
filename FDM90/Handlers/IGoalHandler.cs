using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDM90.Handlers
{
    public interface IGoalHandler
    {
        void CreateGoal(Guid userId, string name, string weekStart, string weekEnd, string targets);

        Task<bool> DailyUpdate();
    }
}

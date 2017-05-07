using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDM90.Handlers
{
    public interface IGoalHandler
    {
        void CreateGoal(Guid userId, string name, DateTime weekStart, DateTime weekEnd, string targets)
    }
}

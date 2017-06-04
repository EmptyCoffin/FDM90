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
        IJEnumerable<JToken> GetGoalInfo(Guid userId, DateTime startDate, DateTime endDate);
    }
}

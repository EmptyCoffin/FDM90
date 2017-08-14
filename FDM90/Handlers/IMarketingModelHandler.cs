using FDM90.Models;
using System.Collections.Generic;

namespace FDM90.Handlers
{
    public interface IMarketingModelHandler
    {
        IEnumerable<MarketingModel> GetAllMarketingModels();
    }
}
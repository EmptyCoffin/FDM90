using FDM90.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDM90.Handlers
{
    public interface ILinkedInHandler : IMediaHandler
    {
        string GetLoginUrl();
        Task SetAccessToken(Guid userId, string authorizationCode);
    }
}

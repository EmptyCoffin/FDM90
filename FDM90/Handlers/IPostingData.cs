using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDM90.Handlers
{
    public interface IPostingData
    {
        string CheckPostText(string textToPost, string medias, Guid userId = new Guid());
    }
}

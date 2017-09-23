using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDM90.Models.Helpers
{
    public interface IFileHelper
    {
        void DeleteFile(string path);
        void CreateDirectory(string path);
    }
}

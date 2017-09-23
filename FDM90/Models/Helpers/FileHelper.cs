using FDM90.Singleton;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Web;

namespace FDM90.Models.Helpers
{
    [ExcludeFromCodeCoverage]
    public class FileHelper : IFileHelper
    {
        public void DeleteFile(string path)
        {
            if(File.Exists(ConfigSingleton.Instance.AppPath + path))
            {
                File.Delete(ConfigSingleton.Instance.AppPath + path);
            }
        }

        public void CreateDirectory(string path)
        {
            if (!Directory.Exists(ConfigSingleton.Instance.AppPath + path))
            {
                Directory.CreateDirectory(ConfigSingleton.Instance.AppPath + path);
            }
        }
    }
}
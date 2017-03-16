using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace FDM90.Singleton
{
    public class ConfigSingleton
    {
        public static string FacebookClientId { get { return ConfigurationManager.AppSettings["FacebookClientId"]; } }
        public static string FacebookClientSecret { get { return ConfigurationManager.AppSettings["FacebookClientId"]; } }
    }
}
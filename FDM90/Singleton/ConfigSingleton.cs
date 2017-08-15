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
        public static string FacebookClientSecret { get { return ConfigurationManager.AppSettings["FacebookClientSecret"]; } }
        public static string TwitterConsumerKey { get { return ConfigurationManager.AppSettings["TwitterConsumerKey"]; } }
        public static string TwitterConsumerSecret { get { return ConfigurationManager.AppSettings["TwitterConsumerSecret"]; } }
        public static string FileSaveLocation { get { return ConfigurationManager.AppSettings["FileSaveLocation"]; } }
    }
}
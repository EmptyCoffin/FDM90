using FDM90.Models;
using FDM90.Repository;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace FDM90.Singleton
{
    public class ConfigSingleton
    {
        private static IReadAll<ConfigItem> _configItemRepo;
        private static List<ConfigItem> _configList;

        static List<ConfigItem> ConfigList
        {
            get
            {
                if (_configList == null)
                {
                    new ConfigSingleton();
                }

                return _configList;
            }
            set
            {
                _configList = value;
            }
        }
           

        public ConfigSingleton(IReadAll<ConfigItem> configItemRepo)
        {
            _configList = configItemRepo.ReadAll().ToList();
        }

        public ConfigSingleton():this(new ConfigRepository())
        {

        }

        public static string FacebookClientId { get { return ConfigList.FirstOrDefault(x => x.Name == "FacebookClientId").Value; } }
        public static string FacebookClientSecret { get { return ConfigList.FirstOrDefault(x => x.Name == "FacebookClientSecret").Value; } }
        public static string TwitterConsumerKey { get { return ConfigList.FirstOrDefault(x => x.Name == "TwitterConsumerKey").Value; } }
        public static string TwitterConsumerSecret { get { return ConfigList.FirstOrDefault(x => x.Name == "TwitterConsumerSecret").Value; } }
        public static string FileSaveLocation { get { return ConfigList.FirstOrDefault(x => x.Name == "FileSaveLocation").Value; } }
    }
}
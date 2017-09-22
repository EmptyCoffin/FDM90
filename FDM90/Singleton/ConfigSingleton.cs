using FDM90.Models;
using FDM90.Repository;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace FDM90.Singleton
{
    public class ConfigSingleton
    {
        private static IReadAll<ConfigItem> _configItemRepo;
        private static ConfigSingleton _instance;

        public static ConfigSingleton Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ConfigSingleton();

                return _instance;
            }
        }


        static List<ConfigItem> ConfigList
        {
            get
            {
                return _configItemRepo.ReadAll().ToList();
            }
        }


        public ConfigSingleton(IReadAll<ConfigItem> configItemRepo)
        {
            _configItemRepo = configItemRepo;
        }

        public ConfigSingleton():this(new ConfigRepository())
        {

        }

        public string FacebookClientId { get { return ConfigList.FirstOrDefault(x => x.Name == "FacebookClientId").Value; } }
        public string FacebookClientSecret { get { return ConfigList.FirstOrDefault(x => x.Name == "FacebookClientSecret").Value; } }
        public string TwitterConsumerKey { get { return ConfigList.FirstOrDefault(x => x.Name == "TwitterConsumerKey").Value; } }
        public string TwitterConsumerSecret { get { return ConfigList.FirstOrDefault(x => x.Name == "TwitterConsumerSecret").Value; } }
        public string FileSaveLocation {
            get {
                if (!Directory.Exists(ConfigList.FirstOrDefault(x => x.Name == "FileSaveLocation").Value))
                {
                    Directory.CreateDirectory(ConfigList.FirstOrDefault(x => x.Name == "FileSaveLocation").Value);
                }

                return ConfigList.FirstOrDefault(x => x.Name == "FileSaveLocation").Value;
            }
        }
    }
}
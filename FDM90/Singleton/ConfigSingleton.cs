using FDM90.Models;
using FDM90.Models.Helpers;
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
        private static IFileHelper _fileHelper;
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


        public ConfigSingleton(IReadAll<ConfigItem> configItemRepo, IFileHelper fileHelper)
        {
            _configItemRepo = configItemRepo;
            _fileHelper = fileHelper;
        }

        public ConfigSingleton():this(new ConfigRepository(), new FileHelper())
        {

        }

        public string FacebookClientId { get { return ConfigList.FirstOrDefault(x => x.Name == "FacebookClientId").Value; } }
        public string FacebookClientSecret { get { return ConfigList.FirstOrDefault(x => x.Name == "FacebookClientSecret").Value; } }
        public string TwitterConsumerKey { get { return ConfigList.FirstOrDefault(x => x.Name == "TwitterConsumerKey").Value; } }
        public string TwitterConsumerSecret { get { return ConfigList.FirstOrDefault(x => x.Name == "TwitterConsumerSecret").Value; } }
        public string AppPath { get { return ConfigList.FirstOrDefault(x => x.Name == "AppPath").Value; } }
        public string FileSaveLocation {
            get {
                _fileHelper.CreateDirectory(ConfigList.FirstOrDefault(x => x.Name == "FileSaveLocation").Value.Replace('~', '\\'));
                return ConfigList.FirstOrDefault(x => x.Name == "FileSaveLocation").Value;
            }
        }
    }
}
using FDM90.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FDM90.Singleton
{
    public class EthicalSingleton
    {
        private static IReadAll<string> _profanityRepo;
        private static EthicalSingleton _instance;

        public static EthicalSingleton Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new EthicalSingleton();

                return _instance;
            }
        }


        public List<string> ProfanityList
        {
            get
            {
                return _profanityRepo.ReadAll().ToList();
            }
        }


        public EthicalSingleton(IReadAll<string> profanityRepo)
        {
            _profanityRepo = profanityRepo;
        }

        public EthicalSingleton():this(new ProfanityRepository())
        {

        }

    }
}
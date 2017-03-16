using FDM90.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FDM90.Singleton
{
    public class UserSingleton
    {
        #region Private Properties
        private static UserSingleton _instance;

        #endregion

        #region Public Properties
        public static UserSingleton Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new UserSingleton();

                return _instance;
            }
        }

        public User CurrentUser;

        #endregion

        #region Constructors
        private UserSingleton() { }

        #endregion

    }
}
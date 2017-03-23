using FDM90.Models;
using FDM90.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Reflection;
using FDM90.Models.Helpers;

namespace FDM90.Handlers
{
    public class UserHandler : IUserHandler
    {
        private IRepository<User> _userRepo;
        private IReadSpecific<User> _userSpecific;

        public UserHandler():this(new UserRepository())
        {

        }

        public UserHandler(IRepository<User> userRepo)
        {
            _userRepo = userRepo;
            _userSpecific = (IReadSpecific<User>)userRepo;
        }

        public User RegisterUser(string userName, string emailAddress, string password)
        {
            User newUser = null;
            try
            {
                newUser = new User()
                {
                    UserName = userName,
                    EmailAddress = emailAddress,
                    Password = EncryptionHelper.EncryptString(password)
                };

                //perform checks
                newUser.UserId = Guid.NewGuid();
                //encrypt password

                //write to db
                _userRepo.Create(newUser);
            }
            catch(Exception ex)
            {

            }
            return newUser;
        }

        public User LoginUser(User user)
        {
            //Check user exists
            var test = _userSpecific.ReadSpecific(user.UserName);

            if (test == null)
            {
                test.UserName = "User doesn't exist";
            }

            //check password
            if (EncryptionHelper.DecryptString(test.Password) != user.Password)
            {
                test.UserName = "Password is incorrect";
            }

            //login
            return test;
        }

        public User UpdateUserMediaActivation(User user, string socialMedia)
        {
            User test = user;
            try
            {
                test = _userSpecific.ReadSpecific(user.UserId.ToString());

                foreach (PropertyInfo property in test.GetType().GetProperties())
                {
                    if (property.Name.Contains(socialMedia) && !(bool)property.GetValue(test))
                    {
                        property.SetValue(test, true);
                    }
                }

                _userRepo.Update(test);
            }
            catch(Exception ex)
            {

            }
            return test;
        }
    }
}
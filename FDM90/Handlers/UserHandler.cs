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
                    UserId = Guid.NewGuid(),
                    UserName = userName,
                    EmailAddress = emailAddress,
                    Password = EncryptionHelper.EncryptString(password)
                };

                //write to db
                _userRepo.Create(newUser);
            }
            catch (Exception ex)
            {

            }
            return newUser;
        }

        public User LoginUser(User loginUser)
        {
            //Check user exists
            var user = _userSpecific.ReadSpecific(loginUser.UserName);

            if (user.UserId.Equals(Guid.Empty))
            {
                user.UserName = "User doesn't exist";
            }
            else if (EncryptionHelper.DecryptString(user.Password) != loginUser.Password)
            {
                //check password
                user.UserName = "Password is incorrect";
            }

            //login
            return user;
        }

        public User UpdateUserMediaActivation(User user, string socialMedia)
        {
            User currentUser = new User();
            try
            {
                currentUser = _userSpecific.ReadSpecific(user.UserId.ToString());
                bool updated = false;

                foreach (PropertyInfo property in currentUser.GetType().GetProperties())
                {
                    if (property.Name.Contains(socialMedia))
                    {
                        property.SetValue(currentUser, true);
                        updated = true;
                    }
                }

                if(updated)
                    _userRepo.Update(currentUser);
            }
            catch(Exception ex)
            {

            }
            return currentUser;
        }

        public User GetUser(string userId)
        {
            return _userSpecific.ReadSpecific(userId);
        }

        public void UpdateUser(User user)
        {
            _userRepo.Update(user);
        }
    }
}
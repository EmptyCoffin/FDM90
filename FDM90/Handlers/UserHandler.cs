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
        private IReadSpecific<User> _userReadSpecific;

        public UserHandler() : this(new UserRepository())
        {

        }

        public UserHandler(IRepository<User> userRepo)
        {
            _userRepo = userRepo;
            _userReadSpecific = (IReadSpecific<User>)userRepo;
        }

        public User RegisterUser(string userName, string emailAddress, string password)
        {
            User newUser = null;
            newUser = new User()
            {
                UserId = Guid.NewGuid(),
                UserName = userName,
                EmailAddress = emailAddress,
                Password = EncryptionHelper.EncryptString(password)
            };

            //write to db
            _userRepo.Create(newUser);
            return newUser;
        }

        public User LoginUser(User loginUser)
        {
            //Check user exists
            var user = _userReadSpecific.ReadSpecific(loginUser);

            if (user == null)
            {
                user = new User();
                user.UserName = "User doesn't exist";
            }
            else if (EncryptionHelper.DecryptString(user.Password) != loginUser.Password)
            {
                //check password
                user = new User();
                user.UserName = "Password is incorrect";
            }

            //login
            return user;
        }

        public User UpdateUserMediaActivation(User user, string socialMedia, bool active)
        {
            User currentUser = new User();
            currentUser = _userReadSpecific.ReadSpecific(user);
            bool updated = false;

            foreach (PropertyInfo property in currentUser.GetType().GetProperties())
            {
                if (property.Name.Contains(socialMedia))
                {
                    property.SetValue(currentUser, active);
                    updated = true;
                }
            }

            if (updated)
                _userRepo.Update(currentUser);

            return currentUser;
        }

        public User GetUser(string userId)
        {
            return _userReadSpecific.ReadSpecific(new User(Guid.Parse(userId)));
        }

        public void UpdateUser(User user)
        {
            _userRepo.Update(user);
        }

        public void DeleteUser(Guid userId)
        {
            _userRepo.Delete(new User(userId));
        }
    }
}
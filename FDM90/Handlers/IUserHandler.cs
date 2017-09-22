using FDM90.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDM90.Handlers
{
    public interface IUserHandler
    {
        User RegisterUser(string userName, string emailAddress, string password);
        User LoginUser(User newUser);
        User UpdateUserMediaActivation(User user, string socialMedia, bool active);
        User GetUser(string userId);
        void UpdateUser(User user);
        void DeleteUser(Guid userId);
    }
}

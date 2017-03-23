using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FDM90.Models
{
    public class User
    {
        public User()
        {

        }
        public User(Guid id)
        {
            this.UserId = id;
        }
        public User(string userName, string password)
        {
            this.UserName = userName;
            this.Password = password;
        }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public bool Facebook { get; set; }
    }
}
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

        public List<string> GetIntegratedMediaChannels()
        {
            List<string> channels = new List<string>();
            foreach (var prop in this.GetType().GetProperties())
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    IntegratedMediaChannelAttribute channel = attr as IntegratedMediaChannelAttribute;
                    if ((bool)prop.GetValue(this) == true)
                    {
                        channels.Add(channel.MediaChannelName);
                    }
                }
            }
            return channels;
        }

        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        [IntegratedMediaChannel("Facebook")]
        public bool Facebook { get; set; }
        [IntegratedMediaChannel("Twitter")]
        public bool Twitter { get; set; }
        public int Campaigns { get; set; }
    }

    public class IntegratedMediaChannelAttribute : Attribute
    {
        public string MediaChannelName { get; set; }

        public IntegratedMediaChannelAttribute(string mediaChannelName)
        {
            MediaChannelName = mediaChannelName;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace FDM90.Models.Helpers
{
    public class EncryptionHelper
    {
        public static string EncryptString(string stringToEncrypt)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(stringToEncrypt);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string DecryptString(string stringToDecrypt)
        {
            var base64EncodedBytes = Convert.FromBase64String(stringToDecrypt);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
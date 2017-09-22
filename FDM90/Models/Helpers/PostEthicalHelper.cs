using FDM90.Singleton;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace FDM90.Models.Helpers
{
    public class PostEthicalHelper
    {
        public static string CheckTextForIssues(string textToCheck)
        {
            string errorMessage = "Text Contains: ";

            errorMessage += CheckTextForProfanity(textToCheck);

            return errorMessage == "Text Contains: " ? string.Empty : errorMessage;
        }

        private static string CheckTextForProfanity(string textToCheck)
        {
            string errorMessage = "Profanity: ";

            foreach(var word in EthicalSingleton.Instance.ProfanityList)
            {
                if(Regex.IsMatch(textToCheck, word, RegexOptions.IgnoreCase))
                {
                    errorMessage += word + ',';
                }
            }

            return errorMessage == "Profanity: " ? string.Empty : errorMessage;
        }
    }
}
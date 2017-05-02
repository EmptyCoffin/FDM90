using FDM90.Singleton;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace FDM90.Handlers
{
    public class TwitterHandler : ITwitterHandler
    {
        public async System.Threading.Tasks.Task<string> SaveLoginDetails(Guid UserId)
        {
            // save data to db
            var client = new HttpClient();

            var uri = new Uri("https://api.twitter.com/");

            var encodedConsumerKey = WebUtility.UrlEncode(ConfigSingleton.TwitterConsumerKey);

            var encodedConsumerSecret = WebUtility.UrlEncode(ConfigSingleton.TwitterConsumerSecret);

            var encodedPair = Base64Encode(String.Format("{0}:{1}", encodedConsumerKey, encodedConsumerSecret));

            var requestToken = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(uri, "oauth2/token"),
                Content = new StringContent("grant_type=client_credentials")
            };

            requestToken.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded") { CharSet = "UTF-8" };
            requestToken.Headers.TryAddWithoutValidation("Authorization", String.Format("Basic {0}", encodedPair));

            var bearerResult = await client.SendAsync(requestToken);
            var bearerData = await bearerResult.Content.ReadAsStringAsync();
            var bearerToken = JObject.Parse(bearerData)["access_token"].ToString();

            var requestData = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                //RequestUri = new Uri(uri, apiPath),
            };
            requestData.Headers.TryAddWithoutValidation("Authorization", String.Format("Bearer {0}", bearerToken));

            var results = await client.SendAsync(requestData);
            return await results.Content.ReadAsStringAsync();
        }

        public string AnotherTest()
        {
            string _oauthCallback = Uri.EscapeDataString("http://localhost:1900/Pages/Content/Twitter.aspx");
            string _oauthConsumerKey = Uri.EscapeDataString(ConfigSingleton.TwitterConsumerKey);
            string _oauthConsumerSecret = Uri.EscapeDataString(ConfigSingleton.TwitterConsumerSecret);
            string _oauthNonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(
                                DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture)));

            TimeSpan _timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
            string _oauthTimestamp = Convert.ToInt64(_timeSpan.TotalSeconds).ToString(CultureInfo.InvariantCulture);
            var uri = new Uri("https://api.twitter.com/");
            var url = new Uri(uri, "oauth/request_token");
            string _oauth_Signature = GetSignatureBaseString(_oauthTimestamp, _oauthNonce, url.ToString());
            string _oauth_version = "1.0";

            string Header = "OAuth " +
                            "oauth_callback=" + '"' + _oauthCallback + '"' + "," +
                            "oauth_consumer_key=" + '"' + _oauthConsumerKey + '"' + "," +
                            "oauth_nonce=" + '"' + _oauthNonce + '"' + "," +
                            "oauth_signature= " + '"' + _oauth_Signature + '"' + "," +
                            "oauth_signature_method=" + '"' + "HMAC-SHA1" + '"' + "," +
                            "oauth_timestamp=" + '"' + _oauthTimestamp + '"' + "," +
                            "oauth_version=" + '"' + _oauth_version + '"';

            var client = new HttpClient();
            var requestToken = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = url,
                Content = new StringContent("grant_type=client_credentials")
            };

            requestToken.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded") { CharSet = "UTF-8" };
            requestToken.Headers.TryAddWithoutValidation("Authorization", Header);
            // Need to sort out with client!
            //var bearerResult = await client.SendAsync(requestToken);


            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = "POST";
            httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, Header);
            httpWebRequest.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";

            httpWebRequest.Headers.Add(HttpRequestHeader.AcceptCharset, "UTF-8");
            string test = string.Empty;
            try
            {
                var Result = httpWebRequest.GetResponse();

                test = Result.ToString();
            }
            catch (WebException ex)
            {
                var response = (HttpWebResponse)ex.Response;
            }
            return test;
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string GetSignatureBaseString(string TimeStamp, string Nonce, string url)
        {
            //1.Convert the HTTP Method to uppercase and set the output string equal to this value.
            string Signature_Base_String = "post";
            Signature_Base_String = Signature_Base_String.ToUpper();

            //2.Append the ‘&’ character to the output string.
            Signature_Base_String += "&";

            ////3.Percent encode the URL and append it to the output string.
            Signature_Base_String += Uri.EscapeDataString(url);

            ////4.Append the ‘&’ character to the output string.
            Signature_Base_String += "&";

            //5.append parameter string to the output string.
            Signature_Base_String += Uri.EscapeDataString("oauth_callback") + '=' + Uri.EscapeDataString("http://localhost:1900/Pages/Content/Twitter.aspx") + "&";
            Signature_Base_String += Uri.EscapeDataString("oauth_consumer_key") + '=' + Uri.EscapeDataString(ConfigSingleton.TwitterConsumerKey) + "&";
            Signature_Base_String += Uri.EscapeDataString("oauth_nonce") + '=' + Uri.EscapeDataString(Nonce) + "&";
            Signature_Base_String += Uri.EscapeDataString("oauth_signature_method") + '=' + Uri.EscapeDataString("HMAC-SHA1") + "&";
            Signature_Base_String += Uri.EscapeDataString("oauth_timestamp") + '=' + Uri.EscapeDataString(TimeStamp) + "&";
            Signature_Base_String += Uri.EscapeDataString("oauth_version") + '=' + Uri.EscapeDataString("1.0");

            return Signature_Base_String;
        }

    }
    class AuthenticationResponse
    {
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

    }
}
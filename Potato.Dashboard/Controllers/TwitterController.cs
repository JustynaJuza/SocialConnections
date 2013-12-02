using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Potato.Dashboard.Models.Twitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Potato.Dashboard.Controllers
{
    public class TwitterController : Controller
    {
        public ActionResult Index()
        {
            var authorizationToken = GetTwitterAuthorizationToken();

            //GetTwitterTweet("");
            return View();
        }

        public IList<Tweet> GetTwitterTweet(string userName)
        {
            var requestUri = new Uri("https://api.twitter.com/1.1/statuses/show.json?id=407290840436862976");

            var jsonObject = GetJsonRequestResults(requestUri);
            System.Diagnostics.Debug.WriteLine(jsonObject);
            jsonObject = jsonObject != null ? jsonObject.SelectToken("entry") : null;

            if (jsonObject != null)
            {
                var requestedTweet = JsonConvert.DeserializeObject<Tweet>(jsonObject.ToString());
            }

            // No entries found for this request.
            return null;
        }

        #region HELPERS
        private string GetTwitterAuthorizationToken()
        {
            var consumerKey = "CUb3sO8IPne6a9GPiODrew";
            var consumerSecret = "";

            var bearerTokenCredentials = System.Text.Encoding.ASCII.GetBytes(consumerKey + ":" + consumerSecret);
            var encodedBearerTokenCredentials = Convert.ToBase64String(bearerTokenCredentials);

            var requestHandler = new WebClient();
            requestHandler.Headers.Add("Authorization: Basic " + encodedBearerTokenCredentials);
            requestHandler.Headers.Add("Content-Type: application/x-www-form-urlencoded;charset=UTF-8");

            try
            {
                var jsonString = requestHandler.UploadString(new Uri("https://api.twitter.com/oauth2/token"), "grant_type=client_credentials");
                var jsonObject = JObject.Parse(jsonString);
                return (string) jsonObject.SelectToken("access_token");
            }
            catch (System.Net.WebException ex)
            {
                System.Diagnostics.Debug.WriteLine("WebRequest Error: " + ex.Message);
                return null;
            }
        }

        private JToken GetJsonRequestResults(Uri requestUri)
        {
            var requestHandler = new WebClient();
            requestHandler.Headers.Add("Authorization");
            try
            {
                var jsonString = requestHandler.DownloadString(requestUri);
                return JObject.Parse(jsonString);
                // SelectToken returns child values only! .First/Last return child object with both key+value;
            }
            catch (System.Net.WebException ex)
            {
                System.Diagnostics.Debug.WriteLine("WebRequest Error: " + ex.Message);
                return null;
            }
        }

        private int CorrectRequestResultsCount(int resultsCount)
        {
            if (resultsCount > 50)
            {
                System.Diagnostics.Debug.WriteLine("Error: Twitter can only handle requests for up to 50 results.");
                return 50;
            }

            // Number of request results is correct.
            return resultsCount;
        }
        #endregion HELPERS
    }
}
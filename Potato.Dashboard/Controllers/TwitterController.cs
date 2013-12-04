using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Potato.Dashboard.Models;
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
        CacheProvider cache = new CacheProvider();

        public ActionResult Index()
        {
            #region TWITTER REQUEST SETTINGS
            // Twitter user whose timeline we request.
            string twitterUser = "ITV";
            bool noUserDetailsInTweet = Tweet.noUserDetailsInTweet;
            ViewBag.NoUserDetailsInTweet = noUserDetailsInTweet;
            // How many Tweets should be feteched from timeline.
            // Important: When filtering out replies and retweets you get less than the count, because filtering is after fetching the specific count.
            int timelineResultsCount = 10;
            // Include all user timeline activity?
            bool includeReplies = false;
            bool includeRetweets = false;
            // Index of the first playlist to be fetched (starting with 1).
            string oldestResultId = "";
            // Include a word description of when the video was published.
            bool calculateHowLongSincePublished = true;
            ViewBag.HowLongSincePublished = calculateHowLongSincePublished;
            #endregion TWITTER REQUEST SETTINGS

            var twitterTimeline = new TwitterTimelineViewModel();
            twitterTimeline.User = GetTwitterUser(twitterUser);

            // Show error only if user not found on YouTube.
            if (twitterTimeline.User == null)
            {
                TempData["Error"] = "No user with the name " + twitterUser + " exists on Twitter.";
                return View();
            }

            //twitterTimeline.Tweets.Add(GetTwitterTweet("407543221435510785"));
            twitterTimeline.Tweets = GetTwitterUserTimeline("ITV", false, timelineResultsCount, includeRetweets, includeReplies, oldestResultId, noUserDetailsInTweet, calculateHowLongSincePublished);
            return View(twitterTimeline);
        }

        #region TWITTER REQUEST HANDLING
        public Tweet GetTwitterTweet(string id)
        {
            var requestUri = "statuses/show.json?id=" + id;

            var jsonObject = GetJsonRequestResults(requestUri);
            if (jsonObject != null)
            {
                var requestedTweet = JsonConvert.DeserializeObject<Tweet>(jsonObject.ToString());
                return requestedTweet;
            }

            // No entries found for this request.
            return null;
        }

        public User GetTwitterUser(string userScreenNameOrId)
        {
            var requestUri = "users/show.json?screen_name=" + userScreenNameOrId;

            var jsonObject = GetJsonRequestResults(requestUri);
            if (jsonObject != null)
            {
                var requestedUser = JsonConvert.DeserializeObject<User>(jsonObject.ToString());
                return requestedUser;
            }

            // No entries found for this request.
            return null;
        }

        public IList<Tweet> GetTwitterUserTimeline(string userScreenNameOrId, bool isId = false,
            int resultsCount = 10, bool includeRetweets = true, bool includeReplies = false,
             string oldestResultId = "", bool trimUserDetails = false, bool calculateHowLongSincePublished = false)
        {
            // Correct calls exceeding Twitter handled request results.
            resultsCount = CorrectRequestResultsCount(resultsCount);

            var requestUri = "statuses/user_timeline.json?" + (isId ? "user_id=" : "screen_name=") + userScreenNameOrId
                + "&count=" + resultsCount
                + (includeRetweets ? "&include_rts=1" : "&include_rts=0")
                + (includeReplies ? "" : "&exclude_replies=1")
                + (oldestResultId != "" ? "&since_id=" + oldestResultId : "")
                + (trimUserDetails ? "&trim_user=1" : "");

            var jsonObject = GetJsonRequestResults(requestUri);
            if (jsonObject != null)
            {
                var requestedTweets = new List<Tweet>();
                if (calculateHowLongSincePublished)
                {
                    foreach (var entry in jsonObject)
                    {
                        var requestedTweet = JsonConvert.DeserializeObject<Tweet>(entry.ToString());
                        requestedTweet.calculateHowLongSincePublished();
                        requestedTweets.Add(requestedTweet);
                    }
                }
                else
                {
                    foreach (var entry in jsonObject)
                    {
                        requestedTweets.Add(JsonConvert.DeserializeObject<Tweet>(entry.ToString()));
                    }
                }

                return requestedTweets;
            }

            // No entries found for this request.
            return null;
        }
        #endregion TWITTER REQUEST HANDLING

        #region HELPERS
        private string GetTwitterAuthorizationToken()
        {
            System.Diagnostics.Debug.WriteLine("Requesting Twitter authorization token");
            // Twitter registered application details.
            var consumerKey = "CUb3sO8IPne6a9GPiODrew";
            var consumerSecret = "exsSve7RelWY5RuTq7r26JKb6TkABDEnpMTmP8Fzyw";

            // Encoding application details for application-only authorization token request.
            var bearerTokenCredentials = System.Text.Encoding.ASCII.GetBytes(consumerKey + ":" + consumerSecret);
            var encodedBearerTokenCredentials = Convert.ToBase64String(bearerTokenCredentials);

            var requestHandler = new WebClient();
            // Format request header including application credentials.
            requestHandler.Headers.Add("Authorization: Basic " + encodedBearerTokenCredentials);
            requestHandler.Headers.Add("Content-Type: application/x-www-form-urlencoded;charset=UTF-8");

            try
            {
                // Send Twitter-formated request to retrieve permanent authorization token (invalidated only on request).
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

        private JToken GetJsonRequestResults(string requestUri)
        {
            // Check for cached authorization token or retrieve and save.
            var authorizationToken = cache.GetOrSet("TwitterAuthorizationToken", GetTwitterAuthorizationToken);

            var requestHandler = new WebClient();
            requestHandler.BaseAddress = "https://api.twitter.com/1.1/";
            // All Twitter requests need to be authenticated, GET may be using application-only authorization token.
            requestHandler.Headers.Add("Authorization: Bearer " + authorizationToken);

            try
            {
                var jsonString = requestHandler.DownloadString(requestUri);
                if (jsonString[0] == '{')
                {
                    return JObject.Parse(jsonString);
                }
                return JArray.Parse(jsonString);
            }
            catch (System.Net.WebException ex)
            {
                System.Diagnostics.Debug.WriteLine("WebRequest Error: " + ex.Message);
                return null;
            }
        }

        private int CorrectRequestResultsCount(int resultsCount)
        {
            if (resultsCount > 200)
            {
                System.Diagnostics.Debug.WriteLine("Error: Twitter can only handle requests for up to 200 results.");
                return 200;
            }

            // Number of request results is correct.
            return resultsCount;
        }
        #endregion HELPERS
    }
}
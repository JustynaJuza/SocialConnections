using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocialDashboard.Models.Twitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace SocialDashboard.Models
{
    /// <summary>
    /// Provides Twitter requests and response handling for users, tweets and timelines.
    /// </summary>
    public class TwitterProvider : AbstractExtensions
    {
        // The cache object saving the permanent application-only authorization token to prevent sending too many requests.
        CacheProvider cache = new CacheProvider();
        #region TWITTER REQUEST SETTINGS
        /// <summary>
        /// Twitter user whose timeline we request.</summary>
        public string TwitterUser { get; set; }
        /// <summary>
        /// How many tweets should be feteched from timeline.
        /// <para>Important: When filtering out replies and retweets you get less than the count, because filtering is applied after fetching the specific count.</para></summary>
        public int TimelineResultsCount { get; set; }
        /// <summary>
        /// Include user replies in timeline.
        /// <para>Important: If set to false will filter out all replies from requested Tweets and possibly returning less results than expected.</para></summary>
        public bool IncludeReplies { get; set; }
        /// <summary>
        /// Include retweeted Tweets in timeline.
        /// <para>Important: If set to false will filter out all replies from requested Tweets and possibly returning less results than expected.</para></summary>
        public bool IncludeRetweets { get; set; }
        /// <summary>
        /// Oldest Id constraint applied to request. All fetched tweets will be newer than the Tweet with this Id.</summary>
        public string OldestResultId { get; set; }
        /// <summary>
        /// Include only serId in Tweet.</summary>
        public bool NoUserDetailsInTweet { get; set; }
        /// <summary>
        /// Include a word description of when the tweet was published.</summary>
        public bool CalculateHowLongSincePublished { get; set; }
        #endregion TWITTER REQUEST SETTINGS

        #region CONSTRUCTORS
        private TwitterProvider()
        {
            // Default settings.
            TimelineResultsCount = 50;
            IncludeReplies = false;
            IncludeRetweets = false;
            OldestResultId = "";
            NoUserDetailsInTweet = true;
            CalculateHowLongSincePublished = false;
        }

        /// <summary>
        /// Provides requests for the Twitter user's tweets with default settings.
        /// </summary>
        /// <param name="twitterUser">The Twitter user's username.</param>
        /// <param name="timelineResultsCount">The number of tweets returned in request.</param>
        public TwitterProvider(string twitterUser, int timelineResultsCount = 50)
            : this()
        {
            TwitterUser = twitterUser;
            TimelineResultsCount = timelineResultsCount;
        }
        #endregion CONSTRUCTORS

        public TwitterTimelineViewModel GetTwitterUserData(out string errorText)
        {
            var twitterTimeline = new TwitterTimelineViewModel();
            twitterTimeline.User = GetTwitterUser(TwitterUser);

            // Show error only if user not found on YouTube.
            if (twitterTimeline.User == null)
            {
                errorText = "No user with the name " + TwitterUser + " exists on Twitter.";
                return null;
            }

            errorText = null;
            twitterTimeline.Tweets = GetTwitterUserTimeline(TwitterUser, false, TimelineResultsCount, IncludeRetweets, IncludeReplies, OldestResultId, NoUserDetailsInTweet, CalculateHowLongSincePublished);
            return twitterTimeline;
        }

        #region TWITTER REQUEST HANDLING
        public Tweet GetTwitterTweet(string id, bool calculateHowLongSincePublished = false)
        {
            var requestUri = "statuses/show.json?id=" + id;

            var jsonObject = GetJsonRequestResults(requestUri);
            if (jsonObject != null)
            {
                var requestedTweet = JsonConvert.DeserializeObject<Tweet>(jsonObject.ToString());
                if (calculateHowLongSincePublished)
                {
                    requestedTweet.CalculateHowLongSincePublished();
                }
                requestedTweet.LinkEntitiesInTweet();
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
                        requestedTweet.CalculateHowLongSincePublished();
                        requestedTweet.LinkEntitiesInTweet();
                        requestedTweets.Add(requestedTweet);
                    }
                }
                else
                {
                    foreach (var entry in jsonObject)
                    {
                        var requestedTweet = JsonConvert.DeserializeObject<Tweet>(entry.ToString());
                        requestedTweet.LinkEntitiesInTweet();
                        requestedTweets.Add(requestedTweet);
                    }
                }

                return requestedTweets;
            }

            // No entries found for this request.
            return null;
        }
        #endregion TWITTER REQUEST HANDLING

        #region HELPERS
        /// <summary>
        /// Requests and caches the Twitter API application-only authorization token.
        /// </summary>
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

        /// <summary>
        /// Handles YouTube requests with the specified requestUri, returning a JsonObject or JsonArray.
        /// </summary>
        /// <param name="requestUri">The request Uri with query parameters.</param>
        private JToken GetJsonRequestResults(string requestUri)
        {
            // Check for cached authorization token or retrieve and save.
            var authorizationToken = cache.GetOrSet("TwitterAuthorizationToken", GetTwitterAuthorizationToken, new TimeSpan(60, 0, 0, 0), true);

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

        /// <summary>
        /// Correct calls exceeding Twitter handled request results. Returns maximum value (200) if exceeded.
        /// </summary>
        /// <param name="resultsCount">Number of currently expected results.</param>
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
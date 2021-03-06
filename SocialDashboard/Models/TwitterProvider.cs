﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocialAlliance.Models.Twitter;
using SocialAlliance.Models.WebConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace SocialAlliance.Models
{
    /// <summary>
    /// Provides Twitter requests and response handling for users, tweets and timelines.
    /// </summary>
    public class TwitterProvider : AbstractExtensions, ISocialProvider
    {
        // The cache object saving the permanent application-only authorization token to prevent sending too many requests.
        static CacheProvider cache = new CacheProvider();
        #region TWITTER REQUEST SETTINGS
        /// <summary>
        /// Twitter user whose timeline we request.</summary>
        public string User { get; set; }
        /// <summary>
        /// How many tweets should be feteched from timeline.
        /// <para>Important: When filtering out replies and retweets you get less than the count, because filtering is applied after fetching the specific count.</para></summary>
        public int TimelineResultsCount { get; set; }
        /// <summary>
        /// Oldest Id constraint applied to request. All fetched tweets will be newer than the tweet with this id.</summary>
        public string OldestResultId { get; set; }
        /// <summary>
        /// Include user replies in timeline.
        /// <para>Important: If set to false will filter out all replies from requested tweets and possibly returning less results than expected.</para></summary>
        public bool IncludeReplies { get; set; }
        /// <summary>
        /// Include retweeted tweets in timeline.
        /// <para>Important: If set to false will filter out all replies from requested tweets and possibly returning less results than expected.</para></summary>
        public bool IncludeRetweets { get; set; }
        /// <summary>
        /// Trim user details in tweet, leaving only id.</summary>
        public bool IncludeUserDetailsInTweet { get; set; }
        /// <summary>
        /// Include a word description of when the tweet was published.</summary>
        public bool IncludeHowLongSincePublished { get; set; }
        #endregion TWITTER REQUEST SETTINGS

        #region CONSTRUCTORS
        private TwitterProvider()
        {
            // Default settings.
            TimelineResultsCount = 50;
            OldestResultId = "";
            IncludeReplies = false;
            IncludeRetweets = false;
            IncludeUserDetailsInTweet = true;
            IncludeHowLongSincePublished = false;
        }

        /// <summary>
        /// Provides requests for the Twitter user's tweets with default settings.
        /// </summary>
        /// <param name="twitterUser">The Twitter user's username.</param>
        /// <param name="timelineResultsCount">The number of tweets returned in request.</param>
        public TwitterProvider(string twitterUser, int timelineResultsCount = 50, bool includeUserDetailsInTweet = false, bool includeHowLongSincePublished = false)
            : this()
        {
            User = twitterUser;
            TimelineResultsCount = timelineResultsCount;
            IncludeHowLongSincePublished = includeHowLongSincePublished;
            IncludeUserDetailsInTweet = includeUserDetailsInTweet;
        }

        /// <summary>
        /// Provides requests for the Twitter user's tweets configured by the Web.config entry.
        /// </summary>
        /// <param name="config">The configuration handler.</param>
        public TwitterProvider(TwitterProviderConfig config)
        {
            User = config.User;
            TimelineResultsCount = config.TimelineResultsCount;
            OldestResultId = config.OldestResultId;
            IncludeReplies = config.IncludeReplies;
            IncludeRetweets = config.IncludeRetweets;
            IncludeHowLongSincePublished = config.IncludeHowLongSincePublished;
        }
        #endregion CONSTRUCTORS

        /// <summary>
        /// Performs a sample request to the user's Twitter account, testing the connection.
        /// </summary>
        public static string TestUserRequest(string user)
        {
            var requestUri = "users/show.json?screen_name=" + user;
            try
            {
                if (GetJsonRequestResults(requestUri) == null)
                {
                    return "No user with the name " + user + " exists on Twitter.";
                }
            }
            catch (AuthorizationException ex)
            {
                return ex.Message;
            }

            return null;
        }

        public TwitterTimelineViewModel GetTwitterUserData(out string errorText)
        {
            errorText = null;

            var twitterTimeline = new TwitterTimelineViewModel();
            twitterTimeline.User = GetTwitterUser(out errorText);

            // Show error only if user not found on YouTube.
            if (twitterTimeline.User == null)
            {
                errorText = "No user with the name " + User + " exists on Twitter.";
                return null;
            }

            twitterTimeline.Tweets = GetTwitterUserTimeline();
            return twitterTimeline;
        }

        public User GetTwitterUser(out string errorText)
        {
            errorText = null;

            try
            {
                var user = GetTwitterUser(User);

                // Show error only if user not found on YouTube.
                if (user == null)
                {
                    errorText = "No user with the name " + User + " exists on Twitter.";
                    return null;
                }

                return user;
            }
            catch (AuthorizationException ex)
            {
                errorText = ex.Message;
            }

            return null;
        }

        public IList<Tweet> GetTwitterUserTimeline()
        {
            return GetTwitterUserTimeline(User, false,
                TimelineResultsCount, IncludeRetweets, IncludeReplies,
                OldestResultId, !IncludeUserDetailsInTweet, IncludeHowLongSincePublished);
        }

        #region TWITTER REQUEST HANDLING
        public Tweet GetTwitterTweet(string id, bool includeHowLongSincePublished = false)
        {
            var requestUri = "statuses/show.json?id=" + id;
            Tweet.includeUserDetailsInTweet = true;

            var jsonObject = GetJsonRequestResults(requestUri);
            if (jsonObject != null)
            {
                var requestedTweet = JsonConvert.DeserializeObject<Tweet>(jsonObject.ToString());
                if (includeHowLongSincePublished)
                {
                    requestedTweet.IncludeHowLongSincePublished();
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
             string oldestResultId = "", bool trimUserDetails = true, bool includeHowLongSincePublished = false)
        {
            resultsCount = CorrectRequestResultsCount(resultsCount);

            var requestUri = "statuses/user_timeline.json?" + (isId ? "user_id=" : "screen_name=") + userScreenNameOrId
                + "&count=" + resultsCount
                + (includeRetweets ? "&include_rts=1" : "&include_rts=0")
                + (includeReplies ? "" : "&exclude_replies=1")
                + (oldestResultId != "" ? "&since_id=" + oldestResultId : "")
                + (trimUserDetails ? "&trim_user=1" : "");

            Tweet.includeUserDetailsInTweet = !trimUserDetails;
            var jsonObject = GetJsonRequestResults(requestUri);
            if (jsonObject != null)
            {
                var requestedTweets = new List<Tweet>();
                if (includeHowLongSincePublished)
                {
                    foreach (var entry in jsonObject)
                    {
                        var requestedTweet = JsonConvert.DeserializeObject<Tweet>(entry.ToString());
                        requestedTweet.IncludeHowLongSincePublished();
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
            return new List<Tweet>();
        }
        #endregion TWITTER REQUEST HANDLING

        #region HELPERS
        /// <summary>
        /// Requests and caches the Twitter API application-only authorization token.
        /// </summary>
        private static string GetTwitterAuthorizationToken()
        {
            System.Diagnostics.Debug.WriteLine("Requesting Twitter authorization token");
            // Retrieve Twitter registered application credentials from Web.config section.
            var config = SocialAllianceConfig.Read();
            var credentials = config.ReadCredentials(AccountType.twitter);
            if (credentials != null)
            {
                var consumerKey = credentials.ConsumerKey;
                var consumerSecret = credentials.ConsumerSecret;

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
                    // TODO: Maybe raise and handle error for incorrect credentials?
                    System.Diagnostics.Debug.WriteLine("WebRequest Error: " + ex.Message);

                }
            }

            return null;
        }

        /// <summary>
        /// Handles YouTube requests with the specified requestUri, returning a JsonObject or JsonArray.
        /// </summary>
        /// <param name="requestUri">The request Uri with query parameters.</param>
        private static JToken GetJsonRequestResults(string requestUri)
        {
            // Check for cached authorization token or retrieve and save.
            var authorizationToken = cache.GetOrSet("TwitterAuthorizationToken", GetTwitterAuthorizationToken, new TimeSpan(60, 0, 0, 0), true);
            if (authorizationToken == null)
            {
                // Throw credentials error if Twitter did not grant authorization.
                throw new AuthorizationException("The application was unable to recieve Twitter authorization token - check credentials configuration!");
            }

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
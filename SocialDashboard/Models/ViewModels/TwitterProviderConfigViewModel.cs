using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialDashboard.Models
{
    public class TwitterProviderConfigViewModel
    {
        public Guid TimelineId { get; set; }
        /// <summary>
        /// Twitter user whose timeline we request.</summary>
        public string TwitterUser { get; set; }
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
    
        public TwitterProviderConfigViewModel() { }

        public TwitterProviderConfigViewModel(TwitterProviderConfig config)
        {
            TwitterUser = config.TwitterUser;
            TimelineResultsCount = config.TimelineResultsCount;
            OldestResultId = config.OldestResultId;
            IncludeReplies = config.IncludeReplies;
            IncludeRetweets = config.IncludeRetweets;
            IncludeUserDetailsInTweet = config.IncludeUserDetailsInTweet;
            IncludeHowLongSincePublished = config.IncludeHowLongSincePublished;
        }
    }
}
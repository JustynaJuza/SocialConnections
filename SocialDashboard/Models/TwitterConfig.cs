using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace SocialDashboard.Models
{
    /// <summary>
    /// The handler for Web.config &lt;twitter&gt; section, needs to be referenced in main project's Web.config &lt;configSections&gt; as
    /// <para>&lt;section name="twitter" type="SocialDashboard.Models.TwitterConfig, SocialDashboard" /&gt;</para>
    /// </summary>
    public class TwitterConfig : ConfigurationSection
    {
        private readonly static TwitterConfig defaultInstance = new TwitterConfig();
        /// <summary>
        /// Inner class handling credential entries in &lt;twitter&gt; Web.config section.
        /// </summary>
        public class TwitterCredentials : ConfigurationElement
        {
            [ConfigurationProperty("consumerKey", IsRequired = true)]
            public string ConsumerKey
            {
                get { return (string) this["consumerKey"]; }
                set { this["consumerKey"] = value; }
            }
            [ConfigurationProperty("consumerSecret", IsRequired = true)]
            public string ConsumerSecret
            {
                get { return (string) this["consumerSecret"]; }
                set { this["consumerSecret"] = value; }
            }
        }

        [ConfigurationProperty("credentials")]
        public TwitterCredentials Credentials
        {
            get { return (TwitterCredentials) this["credentials"]; }
            set { this["credentials"] = value; }
        }

        [ConfigurationProperty("mergedTimeline", DefaultValue = null, IsRequired = true)]
        [ConfigurationCollection(typeof(TwitterProviderConfig), AddItemName = "add", ClearItemsName = "clear", RemoveItemName = "remove")]
        public TwitterMergedTimeline MergedTimeline
        {
            get { return (TwitterMergedTimeline) this["mergedTimeline"]; }
            set { this["mergedTimeline"] = value; }
        }

        #region CRUD
        public static TwitterConfig Read()
        {
            // Reads from configuration if using a desktop Application (.exe).
            //if (HttpContext.Current == null)
            //    var configuration = ConfigurationManager.OpenExeConfiguration(null);
            var webConfig = WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);
            var currentTwitterConfig = (TwitterConfig) webConfig.GetSection("twitter");
            return currentTwitterConfig;
        }

        public void CreateOrUpdateCredentials()
        {
            var webConfig = WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);
            var currentTwitterConfig = (TwitterConfig) webConfig.GetSection("twitter");

            currentTwitterConfig.Credentials = this.Credentials;
            webConfig.Save(ConfigurationSaveMode.Modified);
        }
        #endregion CRUD
    }

    public class TwitterProviderConfig : ConfigurationElement
    {
        /// <summary>
        /// Twitter user whose timeline we request.</summary>
        [ConfigurationProperty("user", IsRequired = true, IsKey = true)]
        public string TwitterUser
        {
            get { return (string) this["user"]; }
            set { this["user"] = value; }
        }
        /// <summary>
        /// How many tweets should be feteched from timeline.
        /// <para>Important: When filtering out replies and retweets you get less than the count, because filtering is applied after fetching the specific count.</para></summary>
        [ConfigurationProperty("timelineResults", DefaultValue = 50)]
        public int TimelineResultsCount
        {
            get { return (int) this["timelineResults"]; }
            set { this["timelineResults"] = value; }
        }
        /// <summary>
        /// Oldest Id constraint applied to request. All fetched tweets will be newer than the Tweet with this Id.</summary>
        [ConfigurationProperty("oldestResultId")]
        public string OldestResultId
        {
            get { return (string) this["oldestResultId"]; }
            set { this["oldestResultId"] = value; }
        }
        /// <summary>
        /// Include user replies in timeline.
        /// <para>Important: If set to false will filter out all replies from requested Tweets and possibly returning less results than expected.</para></summary>
        [ConfigurationProperty("includeReplies", DefaultValue = false)]
        public bool IncludeReplies
        {
            get { return (bool) this["includeReplies"]; }
            set { this["includeReplies"] = value; }
        }
        /// <summary>
        /// Include retweeted Tweets in timeline.
        /// <para>Important: If set to false will filter out all replies from requested Tweets and possibly returning less results than expected.</para></summary>
        [ConfigurationProperty("includeRetweets", DefaultValue = false)]
        public bool IncludeRetweets
        {
            get { return (bool) this["includeRetweets"]; }
            set { this["includeRetweets"] = value; }
        }
        /// <summary>
        /// Trim user details in tweet, leaving only id.</summary>
        [ConfigurationProperty("includeUserDetailsInTweet", DefaultValue = false)]
        public bool IncludeUserDetailsInTweet
        {
            get { return (bool) this["includeUserDetailsInTweet"]; }
            set { this["includeUserDetailsInTweet"] = value; }
        }
        /// <summary>
        /// Include a word description of when the tweet was published.</summary>
        [ConfigurationProperty("includeHowLongSincePublished", DefaultValue = false)]
        public bool IncludeHowLongSincePublished
        {
            get { return (bool) this["includeHowLongSincePublished"]; }
            set { this["includeHowLongSincePublished"] = value; }
        }
    }

    public class TwitterMergedTimeline : ConfigurationElementCollection, IEnumerable<ConfigurationElement>
    {
        public void Add(TwitterProviderConfig singleUserConfig)
        {
            base.BaseAdd(singleUserConfig);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new TwitterProviderConfig();
        }

        protected override object GetElementKey(ConfigurationElement singleUserConfig)
        {
            return ((TwitterProviderConfig) singleUserConfig).TwitterUser;
        }

        IEnumerator<ConfigurationElement> IEnumerable<ConfigurationElement>.GetEnumerator()
        {
            return this.OfType<TwitterProviderConfig>().GetEnumerator();
        }
    }
}
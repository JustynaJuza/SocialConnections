﻿using SocialAlliance.Models.YouTube;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;

namespace SocialAlliance.Models.WebConfig
{
    /// <summary>
    /// The component's main configuration handler for the &lt;socialAlliance.config&gt section in Web.config,
    /// <para>needs to be referenced in main project's Web.config &lt;configSections&gt; along with all child sections.
    /// <para>&lt;sectionGroup name="socialAlliance.config" type="SocialAlliance.Models.WebConfig, SocialDashboard" /&gt;</para>
    /// </summary>
    public class SocialAllianceConfig : ConfigurationSectionGroup
    {
        [ConfigurationProperty("authorization", IsRequired = true)]
        public AuthorizationConfig Authorization
        {
            get { return (AuthorizationConfig) this.Sections["authorization"]; }
        }

        [ConfigurationProperty("socialTimelines", IsRequired = true)]
        public SocialTimelinesConfig SocialTimelines
        {
            get { return (SocialTimelinesConfig) this.Sections["socialTimelines"]; }
        }

        #region CRUD
        public static SocialAllianceConfig Read()
        {
            // Reads from configuration if using a desktop Application (.exe).
            //if (HttpContext.Current == null)
            //    var configuration = ConfigurationManager.OpenExeConfiguration(null);
            var webConfig = WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);
            var currentConfig = (SocialAllianceConfig) webConfig.GetSectionGroup("socialAlliance.config");
            return currentConfig;
        }

        public CredentialsConfig ReadCredentials(AccountType accountType)
        {
            //var authorizationProperty = typeof(AuthorizationConfig).GetProperty(accountType.ToString(), BindingFlags.IgnoreCase);
            //return (CredentialsConfig) authorizationProperty.GetValue(Authorization);
            return (CredentialsConfig) Authorization.Credentials.FirstOrDefault(c => c.AccountType == accountType);
        }

        public TimelineConfig ReadTimeline(string timelineNameOrId, bool isId = false)
        {
            if (isId)
            {
                return (TimelineConfig) SocialTimelines.Timelines.FirstOrDefault(t => t.Id == timelineNameOrId);
            }
            return (TimelineConfig) SocialTimelines.Timelines.FirstOrDefault(t => t.Name == timelineNameOrId);
        }

        public static void CreateOrUpdateCredentials(CredentialsConfig credentialsConfig)
        {
            var webConfig = WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);
            var currentConfig = (SocialAllianceConfig) webConfig.GetSectionGroup("socialAlliance.config");

            currentConfig.Authorization.Credentials.Remove(credentialsConfig.AccountType);
            currentConfig.Authorization.Credentials.Add(credentialsConfig);
            webConfig.Save(ConfigurationSaveMode.Modified);
        }

        public static void CreateOrUpdateTimeline(TimelineConfig timelineConfig)
        {
            var webConfig = WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);
            var currentConfig = (SocialAllianceConfig) webConfig.GetSectionGroup("socialAlliance.config");

            // A bit unefficient, but I can't figure out any replace method without editing XML.
            currentConfig.SocialTimelines.Timelines.Remove(timelineConfig.Id);
            currentConfig.SocialTimelines.Timelines.Add(timelineConfig);
            webConfig.Save(ConfigurationSaveMode.Modified);
        }

        public static void DeleteCredentials(AccountType accountType)
        {
            var webConfig = WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);
            var currentConfig = (SocialAllianceConfig) webConfig.GetSectionGroup("socialAlliance.config");

            currentConfig.Authorization.Credentials.Remove(accountType);
            webConfig.Save(ConfigurationSaveMode.Modified);
        }

        public static void DeleteTimeline(string timelineId)
        {
            var webConfig = WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);
            var currentConfig = (SocialAllianceConfig) webConfig.GetSectionGroup("socialAlliance.config");

            currentConfig.SocialTimelines.Timelines.Remove(timelineId);
            webConfig.Save(ConfigurationSaveMode.Modified);
        }
        #endregion CRUD
    }

    #region CONFIG SECTIONS
    /// <summary>
    /// The section containing social account credentials, saved in SocialAlliance.Authorization.config.
    /// </summary>
    public class AuthorizationConfig : ConfigurationSection
    {
        [ConfigurationProperty("", IsDefaultCollection = true)]
        //AddItemName = String.Join(",", Enum.GetNames(typeof(AccountType)))
        [ConfigurationCollection(typeof(CredentialsCollectionConfig), AddItemName = "add", ClearItemsName = "clear", RemoveItemName = "remove")]
        public CredentialsCollectionConfig Credentials
        {
            get { return (CredentialsCollectionConfig) this[""]; }
            set { this[""] = value; }
        }

        public AuthorizationConfig()
        {
            SectionInformation.RestartOnExternalChanges = false;
            SectionInformation.ConfigSource = "SocialAlliance.Authorization.config";
        }
    }

    /// <summary>
    /// The section containing configurations for all timelines, saved in SocialAlliance.Timelines.config.
    /// </summary>
    public class SocialTimelinesConfig : ConfigurationSection
    {
        [ConfigurationProperty("", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(TimelineCollectionConfig), AddItemName = "timeline", ClearItemsName = "clear", RemoveItemName = "remove")]
        public TimelineCollectionConfig Timelines
        {
            get { return (TimelineCollectionConfig) this[""]; }
            set { this[""] = value; }
        }

        public SocialTimelinesConfig()
        {
            SectionInformation.RestartOnExternalChanges = false;
            SectionInformation.ConfigSource = "SocialAlliance.Timelines.config";
        }
    }
    #endregion CONFIG SECTIONS

    #region CONFIG ELEMENTS
    /// <summary>
    /// Handler for request authentication credential entries in the component's &lt;authorization&gt; Web.config section.
    /// </summary>
    public class CredentialsConfig : ConfigurationElement
    {
        [ConfigurationProperty("accountType", IsRequired = true)]
        public AccountType AccountType
        {
            get { return (AccountType) this["accountType"]; }
            set { this["accountType"] = value; }
        }
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

    /// <summary>
    /// Handler for timeline configurations in component's &lt;socialTimelines&gt; Web.config section.
    /// </summary>
    public class TimelineConfig : ConfigurationElement
    {
        [StringValidator(MinLength=1, MaxLength=36)]
        [StringLength(36, ErrorMessage = "Please make sure the {0} has a value of {1}-{2} characters.", MinimumLength = 1)]
        [ConfigurationProperty("id", IsRequired = true, DefaultValue = " ")]
        public string Id
        {
            get { return (string) this["id"]; }
            set { this["id"] = value; }
        }

        [StringValidator(MinLength = 0, MaxLength = 36)]
        [StringLength(36, ErrorMessage = "Please make sure the {0} has a value of less than {1} characters.")]
        [ConfigurationProperty("name", DefaultValue = "")]
        public string Name
        {
            get { return (string) this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("merged", IsRequired = true, DefaultValue = true)]
        public bool Merged
        {
            get { return (bool) this["merged"]; }
            set { this["merged"] = value; }
        }

        [ConfigurationProperty("singleUser", IsRequired = true, DefaultValue = false)]
        public bool SingleUser
        {
            get { return (bool) this["singleUser"]; }
            set { this["singleUser"] = value; }
        }

        [ConfigurationProperty("youTubeUsers", IsRequired = false)]
        [ConfigurationCollection(typeof(YouTubeProviderCollectionConfig), AddItemName = "youTube", ClearItemsName = "clear", RemoveItemName = "remove")]
        public YouTubeProviderCollectionConfig YouTubeProviders
        {
            get { return (YouTubeProviderCollectionConfig) this["youTubeUsers"]; }
            set { this["youTubeUsers"] = value; }
        }

        [ConfigurationProperty("twitterUsers", IsRequired = false)]
        [ConfigurationCollection(typeof(TwitterProviderCollectionConfig), AddItemName = "twitter", ClearItemsName = "clear", RemoveItemName = "remove")]
        public TwitterProviderCollectionConfig TwitterProviders
        {
            get { return (TwitterProviderCollectionConfig) this["twitterUsers"]; }
            set { this["twitterUsers"] = value; }
        }
    }

    public class YouTubeProviderConfig : ConfigurationElement, ISocialProviderConfig
    {
        public string TimelineId { get; set; }
        /// <summary>
        /// YouTube user whose videos we request.</summary>
        [StringValidator(MinLength = 1, MaxLength = 36)]
        [StringLength(36, ErrorMessage = "Please make sure the channel/user name has a value of {1}-{2} characters.", MinimumLength = 1)]
        [ConfigurationProperty("user", IsRequired = true, IsKey = true, DefaultValue = " ")]
        public string User
        {
            get { return (string) this["user"]; }
            set { this["user"] = value; }
        }
        /// <summary>
        /// Playlist name for fetching videos from specific playlist.</summary>
        [ConfigurationProperty("playlistTitle", DefaultValue = "")]
        public string PlaylistTitle
        {
            get { return (string) this["playlistTitle"]; }
            set { this["playlistTitle"] = value; }
        }
        /// <summary>
        /// How many results should be feteched for user/playlistId videos.</summary>
        [IntegerValidator(MinValue=0, MaxValue=50)]
        [Range(0, 50, ErrorMessage = "YouTube can only handle requests for up to 50 results.")]
        [ConfigurationProperty("videoResults", DefaultValue = 10)]
        public int VideoResultsCount
        {
            get { return (int) this["videoResults"]; }
            set { this["videoResults"] = value; }
        }
        /// <summary>
        /// How many results should be feteched for playlists.</summary>
        [IntegerValidator(MinValue = 0, MaxValue = 50)]
        [Range(0, 50, ErrorMessage = "YouTube can only handle requests for up to 50 results.")]
        [ConfigurationProperty("playlistResults", DefaultValue = 0)]
        public int PlaylistResultsCount
        {
            get { return (int) this["playlistResults"]; }
            set { this["playlistResults"] = value; }
        }
        /// <summary>
        /// Include videos in found playlists, based on VideoResultsCount and VideosStartResultsIndex. </summary>
        [ConfigurationProperty("includePlaylistVideos", DefaultValue = true)]
        public bool IncludePlaylistVideos
        {
            get { return (bool) this["includePlaylistVideos"]; }
            set { this["includePlaylistVideos"] = value; }
        }
        /// <summary>
        /// Index of the first video to be fetched (starting with 1).</summary>
        [IntegerValidator(MinValue = 1)]
        [Range(1, int.MaxValue, ErrorMessage = "The search index must start from at least 1.")]
        [ConfigurationProperty("videosStartIndex", DefaultValue = 1)]
        public int VideosStartResultsIndex
        {
            get { return (int) this["videosStartIndex"]; }
            set { this["videosStartIndex"] = value; }
        }
        /// <summary>
        /// Index of the first playlist to be fetched (starting with 1).</summary>
        [IntegerValidator(MinValue = 1)]
        [Range(1, int.MaxValue, ErrorMessage = "The search index must start from at least 1.")]
        [ConfigurationProperty("playlistsStartIndex", DefaultValue = 1)]
        public int PlaylistsStartResultsIndex
        {
            get { return (int) this["playlistsStartIndex"]; }
            set { this["playlistsStartIndex"] = value; }
        }
        /// <summary>
        /// The ordering of videos requested from a specific user's uploads.
        /// <para>Important: Live database search (with updated likes, views, etc.) only with 'relevance'.</para></summary>
        [ConfigurationProperty("userVideosOrder", DefaultValue = VideoOrder.published)]
        public VideoOrder UserVideosOrder
        {
            get { return (VideoOrder) this["userVideosOrder"]; } //Enum.Parse(typeof(VideoOrder), (string) this["userVideosOrder"], true); }
            set { this["userVideosOrder"] = value; }
        }
        /// <summary>
        /// The ordering of videos in a playlist.
        /// <para>Important: Live database search (with updated likes, views, etc.) only with 'position'.</para></summary>
        [ConfigurationProperty("playlistVideosOrder", DefaultValue = PlaylistOrder.position)]
        public PlaylistOrder PlaylistVideosOrder
        {
            get { return (PlaylistOrder) this["playlistVideosOrder"]; } //Enum.Parse(typeof(PlaylistOrder), (string) this["playlistVideosOrder"], true); }
            set { this["playlistVideosOrder"] = value; }
        }
        /// <summary>
        /// Include a word description of when the video was published.</summary>
        [ConfigurationProperty("includeHowLongSincePublished", DefaultValue = false)]
        public bool IncludeHowLongSincePublished
        {
            get { return (bool) this["includeHowLongSincePublished"]; }
            set { this["includeHowLongSincePublished"] = value; }
        }
    }

    public class TwitterProviderConfig : ConfigurationElement, ISocialProviderConfig
    {
        public string TimelineId { get; set; }
        /// <summary>
        /// Twitter user whose timeline we request.</summary>
        [StringValidator(MinLength = 1, MaxLength = 36)]
        [StringLength(36, ErrorMessage = "Please make sure the user's screen-name has a value of {1}-{2} characters.", MinimumLength = 1)]
        [ConfigurationProperty("user", IsRequired = true, IsKey = true, DefaultValue = " ")]
        public string User
        {
            get { return (string) this["user"]; }
            set { this["user"] = value; }
        }
        /// <summary>
        /// How many tweets should be feteched from timeline.
        /// <para>Important: When filtering out replies and retweets you get less than the count, because filtering is applied after fetching the specific count.</para></summary>
        [IntegerValidator(MinValue = 0, MaxValue = 200)]
        [Range(0, 200, ErrorMessage = "Twitter can only handle requests for up to 200 results.")]
        [ConfigurationProperty("timelineResults", DefaultValue = 50)]
        public int TimelineResultsCount
        {
            get { return (int) this["timelineResults"]; }
            set { this["timelineResults"] = value; }
        }
        /// <summary>
        /// Oldest Id constraint applied to request. All fetched tweets will be newer than the Tweet with this Id.</summary>
        [ConfigurationProperty("oldestResultId", DefaultValue="")]
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
    #endregion CONFIG ELEMENTS

    #region CONFIG COLLECTIONS
    /// <summary>
    /// The collection storing all social accounts' credential entries in Web.config.
    /// </summary>
    public class CredentialsCollectionConfig : ConfigurationElementCollection, IEnumerable<CredentialsConfig>
    {
        public void Add(CredentialsConfig credentials)
        {
            base.BaseAdd(credentials);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        public void Remove(AccountType accountType)
        {
            BaseRemove(accountType);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new CredentialsConfig();
        }

        protected override object GetElementKey(ConfigurationElement timelineConfig)
        {
            return ((CredentialsConfig) timelineConfig).AccountType;
        }

        IEnumerator<CredentialsConfig> IEnumerable<CredentialsConfig>.GetEnumerator()
        {
            return this.OfType<CredentialsConfig>().GetEnumerator();
        }
    }

    /// <summary>
    /// The collection storing all &lt;timeline&gt; entries in Web.config.
    /// </summary>
    public class TimelineCollectionConfig : ConfigurationElementCollection, IEnumerable<TimelineConfig>
    {
        public void Add(TimelineConfig timeline)
        {
            base.BaseAdd(timeline);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        public void Remove(string timelineId)
        {
            BaseRemove(timelineId);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new TimelineConfig();
        }

        protected override object GetElementKey(ConfigurationElement timelineConfig)
        {
            return ((TimelineConfig) timelineConfig).Id;
        }

        IEnumerator<TimelineConfig> IEnumerable<TimelineConfig>.GetEnumerator()
        {
            return this.OfType<TimelineConfig>().GetEnumerator();
        }
    }

    /// <summary>
    /// The YouTube provider configuration entries in each timeline.
    /// </summary>
    public class YouTubeProviderCollectionConfig : ConfigurationElementCollection, IEnumerable<YouTubeProviderConfig>
    {
        public void Add(YouTubeProviderConfig providerConfig)
        {
            base.BaseAdd(providerConfig);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        public void Remove(string user)
        {
            BaseRemove(user);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new YouTubeProviderConfig();
        }

        protected override object GetElementKey(ConfigurationElement singleUserConfig)
        {
            return ((YouTubeProviderConfig) singleUserConfig).User;
        }

        IEnumerator<YouTubeProviderConfig> IEnumerable<YouTubeProviderConfig>.GetEnumerator()
        {
            return this.OfType<YouTubeProviderConfig>().GetEnumerator();
        }
    }

    /// <summary>
    /// The Twitter provider configuration entries in each timeline.
    /// </summary>
    public class TwitterProviderCollectionConfig : ConfigurationElementCollection, IEnumerable<TwitterProviderConfig>
    {
        public void Add(TwitterProviderConfig providerConfig)
        {
            base.BaseAdd(providerConfig);
        }

        public void Clear()
        {
            base.BaseClear();
        }

        public void Remove(string user)
        {
            BaseRemove(user);
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new TwitterProviderConfig();
        }

        protected override object GetElementKey(ConfigurationElement singleUserConfig)
        {
            return ((TwitterProviderConfig) singleUserConfig).User;
        }

        IEnumerator<TwitterProviderConfig> IEnumerable<TwitterProviderConfig>.GetEnumerator()
        {
            return this.OfType<TwitterProviderConfig>().GetEnumerator();
        }
    }
    #endregion CONFIG COLLECTIONS

    
  // [RegularExpression(@"^.{11}$ | ^$", ErrorMessage = "YouTube Id's are exactly {2} characters long, update the {0}.")]
}
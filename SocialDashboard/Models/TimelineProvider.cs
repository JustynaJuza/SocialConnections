using SocialAlliance.Models.Twitter;
using SocialAlliance.Models.WebConfig;
using SocialAlliance.Models.YouTube;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace SocialAlliance.Models
{
    /// <summary>
    /// Provides timeline view models or error messages if request errors occur.
    /// </summary>
    public class TimelineProvider
    {
        public bool HowLongSincePublished { get; set; }
        public bool UserDetailsInTweet { get; set; }
        public string ErrorText { get; set; }

        /// <summary>
        /// Provides a timeline view model populated by using the configuration for the given timeline.
        /// </summary>
        public TimelineViewModel PrepareTimelineViewModel(string TimelineNameOrId, bool isId = false)
        {
            var config = SocialAllianceConfig.Read();
            var timelineConfig = config.ReadTimeline(TimelineNameOrId, isId);

            if (timelineConfig == null)
            {
                ErrorText = "There is no timeline configured with this name or Id: " + TimelineNameOrId;
                return null;
            }

            var timeline = new TimelineViewModel(timelineConfig);
            // TODO: Get YouTube timelines from config.
            List<YouTubeProvider> youTubeProviders = GetYouTubeProvidersFromConfig(timelineConfig.YouTubeProviders);
            List<TwitterProvider> twitterProviders = GetTwitterProvidersFromConfig(timelineConfig.TwitterProviders);



            // If providerConfig configured for any social account, fetch data.
            if (youTubeProviders.Any() || twitterProviders.Any())
            {
                if (youTubeProviders.Count > 1 && timeline.SingleUser == true)
                {
                    ErrorText = "The timeline was specified to be for only one user, but more than one YouTube feed was requested.";
                    timeline.SingleUser = false;
                }
                if (twitterProviders.Count > 1 && timeline.SingleUser == true)
                {
                    ErrorText = "The timeline was specified to be for only one user, but more than one Twitter timeline feed was requested.";
                    timeline.SingleUser = false;
                }

                if (!timeline.SingleUser)
                {
                    // Update the setting for user details in tweets for both Twitter request calls and display settings.
                    UserDetailsInTweet = CorrectIncludeUserDetailsInTweet(twitterProviders);
                }

                // TODO: Optional? Modify to show publish time in tweets only?
                var providers = new List<ISocialProvider>(youTubeProviders).Concat(twitterProviders).ToList();
                HowLongSincePublished = HowLongSincePublishedConsistency(providers);

                if (timeline.SingleUser)
                {
                    timeline = SingleUserTimelineViewModel(timeline, youTubeProviders.ElementAt(0), twitterProviders.ElementAt(0));
                }
                else
                {
                    timeline = MultipleUsersTimelineViewModel(timeline, youTubeProviders, twitterProviders);
                }
            }

            return timeline;
        }
        /// <summary>
        /// Prepares a merged or unmerged timeline including user information.
        /// </summary>
        private TimelineViewModel SingleUserTimelineViewModel(TimelineViewModel timeline, YouTubeProvider youTube, TwitterProvider twitter)
        {
            if (youTube != null)
            {
                var youTubeErrorText = "";
                timeline.YouTubeAccount = youTube.GetYouTubeUserData(out youTubeErrorText);
                ErrorText += youTubeErrorText;
            }
            if (twitter != null)
            {
                var twitterErrorText = "";
                timeline.TwitterAccount = twitter.GetTwitterUserData(out twitterErrorText);
                ErrorText += twitterErrorText;
            }

            // If user-related request errors exist, return without attempting to load the timeline data.
            if (ErrorText != "")
            {
                return null;
            }
            else
            {
                ErrorText = null;
            }

            if (timeline.Merged)
            {
                var mergedTimeline = new List<ISocialEntry>(timeline.TwitterAccount.Tweets);
                mergedTimeline = mergedTimeline.Concat(timeline.YouTubeAccount.Playlists.ElementAt(0).Entries).ToList();
                mergedTimeline.Sort(new SocialEntriesRecentDateFirstComparer());
                timeline.RecentActivity = mergedTimeline;
            }
            else
            {
                timeline.TwitterAccount.Tweets.ToList().Sort(new SocialEntriesRecentDateFirstComparer());
                timeline.YouTubeAccount.Playlists.ElementAt(0).Entries.ToList().Sort(new SocialEntriesRecentDateFirstComparer());
            }

            return timeline;
        }
        /// <summary>
        /// Prepares a merged or unmerged timeline for multiple users.
        /// </summary>
        private TimelineViewModel MultipleUsersTimelineViewModel(TimelineViewModel timeline, IList<YouTubeProvider> youTubeProviders, IList<TwitterProvider> twitterProviders)
        {
            var youTubeTimeline = new List<Video>();
            var twitterTimeline = new List<Tweet>();

            foreach (var entry in youTubeProviders)
            {
                youTubeTimeline = youTubeTimeline.Concat(entry.GetChannelVideos()).ToList();
            }
            foreach (var entry in twitterProviders)
            {
                twitterTimeline = twitterTimeline.Concat(entry.GetTwitterUserTimeline()).ToList();
            }

            if (timeline.Merged)
            {
                var mergedTimeline = new List<ISocialEntry>(twitterTimeline).Concat(youTubeTimeline).ToList();
                mergedTimeline.Sort(new SocialEntriesRecentDateFirstComparer());
                timeline.RecentActivity = mergedTimeline;
            }
            else
            {
                twitterTimeline.Sort(new SocialEntriesRecentDateFirstComparer());
                youTubeTimeline.Sort(new SocialEntriesRecentDateFirstComparer());
                timeline.TwitterAccount = new TwitterTimelineViewModel(twitterTimeline);
                var playlist = new List<Playlist>();
                playlist.Add(new Playlist(youTubeTimeline));
                timeline.YouTubeAccount = new YouTubeVideoChannelViewModel(playlist);
            }

            return timeline;
        }

        #region PROVIDER CONFIG HANDLING
        /// <summary>
        /// Get a list of YouTube providerConfig configurations from Web.config.
        /// </summary>
        private List<YouTubeProvider> GetYouTubeProvidersFromConfig(ConfigurationElementCollection youTubeProviderConfigs)
        {
            if (youTubeProviderConfigs.Count > 0)
            {
                var youTubeProviders = new List<YouTubeProvider>();
                foreach (YouTubeProviderConfig entry in youTubeProviderConfigs)
                {
                    youTubeProviders.Add(new YouTubeProvider(entry));
                }

                return youTubeProviders;
            }

            return new List<YouTubeProvider>();
            //{
            //    new YouTubeProvider("ITV1", 5),
            //    new YouTubeProvider("paulsoaresjr", 5)
            //};
        }

        /// <summary>
        /// Get a list of Twitter providerConfig configurations from Web.config.
        /// </summary>
        private List<TwitterProvider> GetTwitterProvidersFromConfig(ConfigurationElementCollection twitterProviderConfigs)
        {
            if (twitterProviderConfigs.Count > 0)
            {
                var twitterProviders = new List<TwitterProvider>();
                foreach (TwitterProviderConfig entry in twitterProviderConfigs)
                {
                    twitterProviders.Add(new TwitterProvider(entry));
                }

                return twitterProviders;
            }

            return new List<TwitterProvider>();
            //{
            //    new TwitterProvider("ITV", 20, true, true), 
            //    new TwitterProvider("BigReunionITV", 20, true, true), 
            //    new TwitterProvider("ITV2", 20, true, true)
            //};
        }
        #endregion PROVIDER CONFIG HANDLING

        #region HELPERS
        /// <summary>
        /// Make sure user details are included in tweets if more than one user is merged into timeline.
        /// </summary>
        /// <param name="twitterProviders">The list of Twitter providers included in the merged timeline.</param>
        private bool CorrectIncludeUserDetailsInTweet(IList<TwitterProvider> twitterProviders)
        {
            // Take the value of the first element as sample.
                foreach (var provider in twitterProviders)
                {
                    provider.IncludeUserDetailsInTweet = true;
                }
                return true;
        }

        /// <summary>
        /// Apply consistency to publish time calculation.
        /// <para>(All timelines calculate descriptive time and it is displayed, or disable calculating entirely).</para>
        /// </summary>
        /// <param name="providers">The list of providers included in the merged timeline.</param>
        private bool HowLongSincePublishedConsistency(IList<ISocialProvider> providers)
        {
            // Take the value of the first element as sample.
            var includeHowLongSincePublished = providers.ElementAt(0).IncludeHowLongSincePublished;
            if (providers.Count > 1)
            {
                if (includeHowLongSincePublished == false)
                {
                    // If one providerConfig doesn't include publish string, don't calculate it in any other providers.
                    // Rule: performance over majority, if one is inconsistent, prefer performace and skip calculation.
                    for (int i = 1; i < providers.Count; i++)
                    {
                        providers.ElementAt(i).IncludeHowLongSincePublished = false;
                    }
                    return false;
                }
                else if (providers.Count > 1)
                {
                    for (int i = 1; i < providers.Count; i++)
                    {
                        if (providers.ElementAt(i).IncludeHowLongSincePublished != includeHowLongSincePublished)
                        {
                            // Found an inconsistent entry, apply rule as in comment above.
                            foreach (var provider in providers)
                            {
                                provider.IncludeHowLongSincePublished = false;
                            }
                            return false;
                        }
                    }
                }
                return true;
            }

            return includeHowLongSincePublished;
        }
        #endregion HELPERS

    }
}
using SocialDashboard.Models;
using SocialDashboard.Models.Twitter;
using SocialDashboard.Models.YouTube;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Potato.Dashboard.Controllers
{
    public class HomeController : Controller
    {
        public JsonResult MagicTrick()
        {
            CacheProvider cache = new CacheProvider();
            cache.Remove("TwitterAuthorizationToken");

            return new JsonResult()
            {
                Data = "You-know-what removed from you-know-where...",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult Index()
        {
            // Returns a SocialDashboardViewModel for the main View.
            return View("SocialDashboard", PrepareDashboardViewModel(false));
        }

        public ActionResult SplitSocialDashboard()
        {
            // Returns a SocialDashboardViewModel for the separate column View.
            return View("SocialDashboard", PrepareDashboardViewModel(false, false));
        }

        public ActionResult TimelineConfig()
        {
            TwitterConfig twitterConfig = TwitterConfig.Read();
            var currentProviderConfig = new List<TwitterProviderConfigViewModel>();
            foreach (TwitterProviderConfig entry in twitterConfig.MergedTimeline)
            {
                currentProviderConfig.Add(new TwitterProviderConfigViewModel(entry));
            }

            var timelineConfig = new TimelineConfigViewModel()
            {
                Id = Guid.NewGuid(),
                TwitterProviderConfig = currentProviderConfig,
                MergedTimeline = true
            };
            return View(timelineConfig);
        }

        [HttpPost]
        public ActionResult Delete(string userName)
        {
            return View();
        }


        #region VIEWMODEL HANDLING
        /// <summary>
        /// Returns a new SocialDashboardViewModel for the Dashboard View.
        /// </summary>
        private SocialDashboardViewModel PrepareDashboardViewModel(bool singleUser, bool mergedTimeline = true)
        {
            SocialDashboardViewModel socialDashboard = null;
            // TODO: Get YouTube timelines from config.
            List<YouTubeProvider> youTubeProviders = GetYouTubeProvidersFromConfig();
            List<TwitterProvider> twitterProviders = GetTwitterProvidersFromConfig();

            // If provider configured for any social account, fetch data.
            if (youTubeProviders.Any() || twitterProviders.Any())
            {
                if (youTubeProviders.Count > 1 && singleUser == true)
                {
                    TempData["Error"] = "The timeline was specified to be for only one user, but more than one YouTube feed was requested.";
                    singleUser = false;
                }
                if (twitterProviders.Count > 1 && singleUser == true)
                {
                    TempData["Error"] = "The timeline was specified to be for only one user, but more than one Twitter timeline feed was requested.";
                    singleUser = false;
                }

                // TODO: Optional? Modify to show publish time in tweets only?
                var providers = new List<IDashboardProvider>(youTubeProviders).Concat(twitterProviders).ToList();
                ViewBag.HowLongSincePublished = HowLongSincePublishedConsistency(providers);

                if (singleUser)
                {
                    socialDashboard = SingleUserSocialDashboardViewModel(mergedTimeline, youTubeProviders.ElementAt(0), twitterProviders.ElementAt(0));
                }
                else
                {
                    socialDashboard = MultipleUsersSocialDashboardViewModel(mergedTimeline, youTubeProviders, twitterProviders);
                }
            }

            ViewBag.MergedTimeline = mergedTimeline;
            ViewBag.SingleUser = singleUser;
            return socialDashboard;
        }

        private SocialDashboardViewModel SingleUserSocialDashboardViewModel(bool mergedTimeline, YouTubeProvider youTube, TwitterProvider twitter)
        {
            var socialDashboard = new SocialDashboardViewModel();

            if (youTube != null)
            {
                var youTubeErrorText = "";
                socialDashboard.YouTubeAccount = youTube.GetYouTubeUserData(out youTubeErrorText);
                TempData["Error"] += (!String.IsNullOrEmpty(youTubeErrorText) ? youTubeErrorText + " " : null);
            }
            if (twitter != null)
            {
                var twitterErrorText = "";
                socialDashboard.TwitterAccount = twitter.GetTwitterUserData(out twitterErrorText);
                TempData["Error"] += (!String.IsNullOrEmpty(twitterErrorText) ? twitterErrorText : null);
            }

            if (mergedTimeline)
            {
                var timeline = new List<IDashboardEntry>(socialDashboard.TwitterAccount.Tweets);
                timeline = timeline.Concat(socialDashboard.YouTubeAccount.Playlists.ElementAt(0).Entries).ToList();
                timeline.Sort(new DashboardEntriesRecentDateFirstComparer());
                socialDashboard.RecentActivity = timeline;
            }
            else
            {
                socialDashboard.TwitterAccount.Tweets.ToList().Sort(new DashboardEntriesRecentDateFirstComparer());
                socialDashboard.YouTubeAccount.Playlists.ElementAt(0).Entries.ToList().Sort(new DashboardEntriesRecentDateFirstComparer());
            }

            return socialDashboard;
        }

        private SocialDashboardViewModel MultipleUsersSocialDashboardViewModel(bool mergedTimeline, IList<YouTubeProvider> youTubeProviders, IList<TwitterProvider> twitterProviders)
        {
            var socialDashboard = new SocialDashboardViewModel();
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

            if (mergedTimeline)
            {
                var timeline = new List<IDashboardEntry>(twitterTimeline).Concat(youTubeTimeline).ToList();
                timeline.Sort(new DashboardEntriesRecentDateFirstComparer());
                socialDashboard.RecentActivity = timeline;
            }
            else
            {
                twitterTimeline.Sort(new DashboardEntriesRecentDateFirstComparer());
                youTubeTimeline.Sort(new DashboardEntriesRecentDateFirstComparer());
                socialDashboard.TwitterAccount = new TwitterTimelineViewModel(twitterTimeline);
                var playlist = new List<Playlist>();
                playlist.Add(new Playlist(youTubeTimeline));
                socialDashboard.YouTubeAccount = new YouTubeVideoChannelViewModel(playlist);
            }

            return socialDashboard;
        }

        /// <summary>
        /// Get a list of YouTube provider configurations from Web.config.
        /// </summary>
        private List<YouTubeProvider> GetYouTubeProvidersFromConfig()
        {
            return new List<YouTubeProvider>()
            {
                new YouTubeProvider("ITV1", 5),
                new YouTubeProvider("paulsoaresjr", 5)
            };
        }

        /// <summary>
        /// Get a list of Twitter provider configurations from Web.config.
        /// </summary>
        private List<TwitterProvider> GetTwitterProvidersFromConfig()
        {
            TwitterConfig twitterConfig = TwitterConfig.Read();
            if (twitterConfig.MergedTimeline.Count > 0)
            {
                var twitterProviders = new List<TwitterProvider>();
                //{
                //    new TwitterProvider("ITV", 20, true, true), 
                //    new TwitterProvider("BigReunionITV", 20, true, true), 
                //    new TwitterProvider("ITV2", 20, true, true)
                //};

                foreach (TwitterProviderConfig entry in twitterConfig.MergedTimeline)
                {
                    twitterProviders.Add(new TwitterProvider(entry));
                }

                // Update the setting for user details in tweets for both Twitter request calls and display settings.
                ViewBag.UserDetailsInTweet = CorrectIncludeUserDetailsInTweet(twitterProviders);

                return twitterProviders;
            }

            return new List<TwitterProvider>();
        }
        #endregion VIEWMODEL HANDLING

        #region HELPERS
        /// <summary>
        /// Make sure user details are included in tweets if more than one user is merged into timeline.
        /// </summary>
        /// <param name="twitterProviders">The list of Twitter providers included in the merged timeline.</param>
        private bool CorrectIncludeUserDetailsInTweet(IList<TwitterProvider> twitterProviders)
        {
            // Take the value of the first element as sample.
            var includeUserDetailsInTweet = twitterProviders.ElementAt(0).IncludeUserDetailsInTweet;
            if (twitterProviders.Count > 1)
            {
                foreach (var provider in twitterProviders)
                {
                    provider.IncludeUserDetailsInTweet = true;
                }
                return true;
            }
            return includeUserDetailsInTweet;
        }

        /// <summary>
        /// Apply consistency to publish time calculation.
        /// <para>(All timelines calculate descriptive time and it is displayed, or disable calculating entirely).</para>
        /// </summary>
        /// <param name="providers">The list of providers included in the merged timeline.</param>
        private bool? HowLongSincePublishedConsistency(IList<IDashboardProvider> providers)
        {
            // Take the value of the first element as sample.
            var includeHowLongSincePublished = providers.ElementAt(0).IncludeHowLongSincePublished;
            if (providers.Count > 1)
            {
                if (includeHowLongSincePublished == false)
                {
                    // If one provider doesn't include publish string, don't calculate it in any other providers.
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
using SocialDashboard.Models;
using SocialDashboard.Models.Twitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Potato.Dashboard.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            // Returns a SocialDashboardViewModel for the main View.
            return View("SocialDashboard", PrepareDashboardViewModel());
        }

        public ActionResult SplitSocialDashboard()
        {
            // Returns a SocialDashboardViewModel for the separate column View.
            return View(PrepareDashboardViewModel());
        }

        public ActionResult TwitterOnly()
        {
            return View();
        }


        /// <summary>
        /// Returns a new SocialDashboardViewModel for the Dashboard View.
        /// </summary>
        private SocialDashboardViewModel PrepareDashboardViewModel()
        {
            var youTube = new YouTubeProvider("ITV1");
            var twitter = new TwitterProvider("ITV");
            youTube.CalculateHowLongSincePublished = twitter.CalculateHowLongSincePublished = true;

            // TODO: Optional?
            ViewBag.HowLongSincePublished = HowLongSincePublishedConsistency(youTube, twitter);
            // Get the global Tweet setting for user details in tweets for both request calls and display settings.
            ViewBag.NoUserDetailsInTweet = twitter.NoUserDetailsInTweet = Tweet.noUserDetailsInTweet;

            // Initialize error placeholders for possible request errors.
            var youTubeErrorText = "";
            var twitterErrorText = "";
            var socialDashboard = new SocialDashboardViewModel();
            socialDashboard.YouTubeAccount = youTube.GetYouTubeUserData(out youTubeErrorText);
            socialDashboard.TwitterAccount = twitter.GetTwitterUserData(out twitterErrorText);

            //socialDashboard.YouTubeAccount.Playlists.ElementAt(0).Entries.Insert(0, youTube.GetVideo("-Cn9NQB1Fug", youTube.CalculateHowLongSincePublished));
            //socialDashboard.TwitterAccount.Tweets.Insert(0, twitter.GetTwitterTweet("409425032344240128"));

            List<IDashboardEntry> dashboard = new List<IDashboardEntry>();
            dashboard = dashboard.Concat(socialDashboard.TwitterAccount.Tweets).ToList();
            dashboard = dashboard.Concat(socialDashboard.YouTubeAccount.Playlists.ElementAt(0).Entries).ToList();
            dashboard.Sort(new DashboardEntriesRecentDateFirstComparer());

            socialDashboard.RecentActivity = dashboard;

            TempData["Error"] = (!String.IsNullOrEmpty(youTubeErrorText) ? youTubeErrorText + " " : (!String.IsNullOrEmpty(twitterErrorText) ? twitterErrorText : null));
            return socialDashboard;
        }

        #region HELPERS
        /// <summary>
        /// Apply consistency to publish time calculation.
        /// <para>(Both social accounts calculate descriptive time and it is displayed, or disable calculating entirely).</para>
        /// </summary>
        /// <param name="youTube">The current YouTubeProvider instance.</param>
        /// <param name="twitter">The current TwitterProvider instance.</param>
        public bool? HowLongSincePublishedConsistency(YouTubeProvider youTube, TwitterProvider twitter)
        {
            if (youTube.CalculateHowLongSincePublished == true && twitter.CalculateHowLongSincePublished == true)
            {
                // If both values are configured to show publish time 
                return true;
            }
            else
            {
                youTube.CalculateHowLongSincePublished = twitter.CalculateHowLongSincePublished = false;
                return null;
            }
        }
        #endregion HELPERS
    }
}
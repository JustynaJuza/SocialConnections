using Potato.Dashboard.Models;
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
            var youTubeErrorText = "";
            var twitterErrorText = "";
            var socialDashboard = new SocialDashboardViewModel();
            var youTube = new YouTubeProvider("ITV1");
            var twitter = new TwitterProvider("ITV");
            youTube.CalculateHowLongSincePublished = twitter.CalculateHowLongSincePublished = true;

            // Apply consistency to publish time calculation.
            // (Both social accounts calculate descriptive time and it is displayed, or disable calculating entirely).
            ViewBag.HowLongSincePublished = HowLongSincePublishedConsistency(youTube, twitter);
            ViewBag.NoUserDetailsInTweet = twitter.NoUserDetailsInTweet;

            socialDashboard.YouTubeAccount = youTube.GetYouTubeUserData(out youTubeErrorText);
            socialDashboard.YouTubeAccount.Playlists.ElementAt(0).Entries.Insert(0, youTube.GetVideo("-Cn9NQB1Fug", youTube.CalculateHowLongSincePublished));
            socialDashboard.TwitterAccount = twitter.GetTwitterUserData(out twitterErrorText);

            TempData["Error"] = (!String.IsNullOrEmpty(youTubeErrorText) ? youTubeErrorText + " " : (!String.IsNullOrEmpty(twitterErrorText) ? twitterErrorText : null));
            return View(socialDashboard);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        #region HELPERS
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
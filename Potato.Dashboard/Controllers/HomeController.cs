using SocialAlliance.Models;
using SocialAlliance.Models.Twitter;
using SocialAlliance.Models.WebConfig;
using SocialAlliance.Models.YouTube;
using System;
using System.Collections.Generic;
using System.Configuration;
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

        public ActionResult HoldingPage()
        {
            ViewBag.NotifyMeMessage = TempData["NotifyMeMessage"];

            return View();
        }

        public ActionResult Index()
        {
            var timelineProvider = new TimelineProvider();
            var viewModel = timelineProvider.PrepareTimelineViewModel("936DA01F-9ABD-4d9d-80C7-02AF85C822A8", true);

            TempData["Error"] = timelineProvider.ErrorText;

            if (viewModel != null)
            {
                ViewBag.HowLongSincePublished = timelineProvider.HowLongSincePublished;
                ViewBag.UserDetailsInTweet = timelineProvider.UserDetailsInTweet;
            }

            // Returns a TimelineViewModel for the main View.
            return View("Timeline", viewModel);
        }

        public ActionResult SplitSocialDashboard()
        {
            var timelineProvider = new TimelineProvider();
            var viewModel = timelineProvider.PrepareTimelineViewModel("0f8fad5b-d9cb-469f-a165-70867728950e", true);

            TempData["Error"] = timelineProvider.ErrorText;

            if (viewModel != null)
            {
                ViewBag.HowLongSincePublished = timelineProvider.HowLongSincePublished;
                ViewBag.UserDetailsInTweet = timelineProvider.UserDetailsInTweet;
            }

            // Returns a TimelineViewModel for the separate column View.
            return View("Timeline", viewModel);
        }
    }
}
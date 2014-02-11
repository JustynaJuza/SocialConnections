using SocialAlliance.Models;
using SocialAlliance.Models.WebConfig;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Potato.Dashboard.Controllers
{
    public class TimelinesController : Controller
    {
        public ActionResult Index()
        {
            var config = SocialAllianceConfig.Read();
            var socialTimelines = config.SocialTimelines.Timelines;

            return View(socialTimelines);
        }

        public ActionResult Configure(string id)
        {
            var config = SocialAllianceConfig.Read();
            var timelineConfig = config.ReadTimeline(id, true);

            return View(timelineConfig);
        }

        #region AJAX ACTIONS
        [HttpPost]
        public ActionResult Add(TimelineConfig timelineConfig)
        {
            if (ModelState.IsValid)
            {
                SocialAllianceConfig.CreateOrUpdateTimeline(timelineConfig);
            }

            var config = SocialAllianceConfig.Read();
            var socialTimelines = config.SocialTimelines.Timelines;

            return PartialView("_TimelinesListPartial", socialTimelines);
        }

        [HttpPost]
        public ActionResult AddTwitterProviderToTimeline(TwitterProviderConfig providerConfig)
        {
            var config = SocialAllianceConfig.Read();
            var timelineConfig = config.ReadTimeline(providerConfig.TimelineId, true);

            if (ModelState.IsValid)
            {
                timelineConfig.TwitterProviders.Add(providerConfig);
                SocialAllianceConfig.CreateOrUpdateTimeline(timelineConfig);
            }

            return PartialView("_TwitterProvidersListPartial", timelineConfig);
        }

        [HttpPost]
        public ActionResult AddYouTubeProviderToTimeline(YouTubeProviderConfig providerConfig)
        {
            var config = SocialAllianceConfig.Read();
            var timelineConfig = config.ReadTimeline(providerConfig.TimelineId, true);

            if (ModelState.IsValid)
            {
                timelineConfig.YouTubeProviders.Add(providerConfig);
                SocialAllianceConfig.CreateOrUpdateTimeline(timelineConfig);
            }

            return PartialView("_YouTubeProvidersListPartial", timelineConfig);
        }

        [HttpPost]
        public ActionResult Update(TimelineConfig newTimelineConfig)
        {
            var config = SocialAllianceConfig.Read();
            var timelineConfig = config.ReadTimeline(newTimelineConfig.Id, true);
            timelineConfig.Name = newTimelineConfig.Name;
            timelineConfig.Merged = newTimelineConfig.Merged;
            timelineConfig.SingleUser = newTimelineConfig.SingleUser;
            SocialAllianceConfig.CreateOrUpdateTimeline(timelineConfig);

            return PartialView("_TimelineDetailsPartial", timelineConfig);
        }

        [HttpPost]
        public ActionResult Test(string user, AccountType accountType)
        {
            if (accountType == AccountType.youTube)
            {
                TempData["Error"] = YouTubeProvider.TestUserRequest(user);
                TempData["Message"] = "Succesful request for YouTube user " + user + ".";
            }
            else
            {
                TempData["Error"] = TwitterProvider.TestUserRequest(user);
                TempData["Message"] = "Succesful request for Twitter user " + user + ".";
            }

            if (TempData["Error"] != null)
            {
                TempData["Message"] = null;
            }

            return PartialView("_MessagePartial");
        }

        [HttpPost]
        public ActionResult Delete(string timelineId)
        {
            SocialAllianceConfig.DeleteTimeline(timelineId);
            var config = SocialAllianceConfig.Read();
            var socialTimelines = config.SocialTimelines.Timelines;

            return PartialView("_TimelinesListPartial", socialTimelines);
        }

        [HttpPost]
        public ActionResult DeleteProvider(string timelineId, string user, AccountType accountType)
        {
            var config = SocialAllianceConfig.Read();
            var timelineConfig = config.ReadTimeline(timelineId, true);

            try
            {
                if (accountType == AccountType.youTube)
                {
                    timelineConfig.YouTubeProviders.Remove(user);
                }
                else
                {
                    timelineConfig.TwitterProviders.Remove(user);
                }
                SocialAllianceConfig.CreateOrUpdateTimeline(timelineConfig);
            }
            catch
            {
                TempData["Error"] = "There was an error deleting the entry, maybe it has already been deleted.";
            }

            return PartialView("_" + accountType.ToString() + "ProvidersListPartial", timelineConfig);
        }
        #endregion AJAX ACTIONS
    }
}
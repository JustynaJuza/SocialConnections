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

        [HttpPost]
        public ActionResult Add(TimelineConfig timelineConfig)
        {
            SocialAllianceConfig.CreateOrUpdateTimeline(timelineConfig);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AddTwitterProviderToTimeline(TwitterProviderConfig providerConfig)
        {
            var config = SocialAllianceConfig.Read();
            var timelineConfig = config.ReadTimeline(providerConfig.TimelineId, true);
            timelineConfig.TwitterProviders.Add(providerConfig);
            SocialAllianceConfig.CreateOrUpdateTimeline(timelineConfig);

            return View("Configure", timelineConfig);
        }

        [HttpPost]
        public ActionResult AddYouTubeProviderToTimeline(YouTubeProviderConfig providerConfig)
        {
            var config = SocialAllianceConfig.Read();
            var timelineConfig = config.ReadTimeline(providerConfig.TimelineId, true);
            timelineConfig.YouTubeProviders.Add(providerConfig);
            SocialAllianceConfig.CreateOrUpdateTimeline(timelineConfig);

            return View("Configure", timelineConfig);
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

            return View("Configure", timelineConfig);
        }

        public ActionResult Edit(int id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult Delete(string timelineId)
        {
            SocialAllianceConfig.CreateOrUpdateTimeline(new TimelineConfig());

            return RedirectToAction("Index");
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

            return View("Configure", timelineConfig);
        }
    }
}

﻿using SocialAlliance.Models.WebConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialAlliance.Models
{
    public class TimelineViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Merged { get; set; }
        public bool SingleUser { get; set; }
        public YouTubeVideoChannelViewModel YouTubeAccount { get; set; }
        public TwitterTimelineViewModel TwitterAccount { get; set; }
        public IList<ISocialEntry> RecentActivity { get; set; }

        public TimelineViewModel(TimelineConfig config)
        {
            Id = config.Id;
            Name = config.Name;
            Merged = config.Merged;
            SingleUser = config.SingleUser;
        }
    }
}
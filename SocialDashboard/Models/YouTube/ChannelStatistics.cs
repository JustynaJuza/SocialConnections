﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialAlliance.Models.YouTube
{
    public class ChannelStatistics : AbstractExtensions
    {
        public int SubscriberCount { get; set; }
        public int TotalUploadViews { get; set; }
    }
}
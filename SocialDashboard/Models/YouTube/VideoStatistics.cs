﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialDashboard.Models.YouTube
{
    public class VideoStatistics : AbstractExtensions
    {
        public int ViewCount { get; set; }
        public int FavouriteCount { get; set; }
    }
}
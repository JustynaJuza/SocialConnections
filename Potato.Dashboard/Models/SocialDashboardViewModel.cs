using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Potato.Dashboard.Models
{
    public class SocialDashboardViewModel
    {
        public YouTubeVideoChannelViewModel YouTubeAccount { get; set; }
        public TwitterTimelineViewModel TwitterAccount { get; set; }
    }
}
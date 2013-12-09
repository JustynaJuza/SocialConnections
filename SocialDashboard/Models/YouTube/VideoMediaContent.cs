using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialDashboard.Models.YouTube
{
    public class VideoMediaContent : AbstractExtensions
    {
        public int Duration { get; set; }
        public string ContentType { get; set; }
        public Uri Source { get; set; }
    }
}
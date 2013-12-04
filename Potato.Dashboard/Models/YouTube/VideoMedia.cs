using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Potato.Dashboard.Models.YouTube
{
    public class VideoMedia : AbstractExtensions
    {
        public string Title { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public IList<string> Keywords { get; set; }
        public Uri Thumbnail { get; set; }
        public VideoMediaContent Content { get; set; }
    }
}
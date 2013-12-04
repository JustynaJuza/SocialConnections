using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Potato.Dashboard.Models.YouTube
{
    public class VideoComments : AbstractExtensions
    {
        public int CommentsCount { get; set; }
        public Uri CommentsFeed { get; set; }
    }
}
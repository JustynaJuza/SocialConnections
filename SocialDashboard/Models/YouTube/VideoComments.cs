using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialDashboard.Models.YouTube
{
    public class VideoComments : AbstractExtensions
    {
        public int CommentsCount { get; set; }
        public Uri CommentsFeed { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Potato.Dashboard.Models.Twitter
{
    public class UserStatistics : AbstractExtensions
    {
        public int FollowersCount { get; set; }
        public int FriendsCount { get; set; }
        public int ListedCount { get; set; }
        public int TweetStatusesCount { get; set; }
    }
}
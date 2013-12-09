using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialDashboard.Models.Twitter
{
    public class UserStatistics : AbstractExtensions
    {
        public int FollowersCount { get; set; }
        public int FriendsCount { get; set; }
        public int ListedCount { get; set; }
        public int TweetStatusesCount { get; set; }
    }
}
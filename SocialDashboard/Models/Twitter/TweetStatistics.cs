using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialAlliance.Models.Twitter
{
    public class TweetStatistics : AbstractExtensions
    {
        public int RetweetsCount { get; set; }
        public int FavouritesCount { get; set; }
    }
}
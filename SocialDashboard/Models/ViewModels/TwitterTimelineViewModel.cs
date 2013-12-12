using SocialDashboard.Models.Twitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialDashboard.Models
{
    public class TwitterTimelineViewModel : AbstractExtensions
    {
        public User User { get; set; }
        public IList<Tweet> Tweets { get; set; }

        public TwitterTimelineViewModel()
        {
            Tweets = new List<Tweet>();
        }

        public TwitterTimelineViewModel(IList<Tweet> tweets)
        {
            Tweets = tweets;
        }
    }
}
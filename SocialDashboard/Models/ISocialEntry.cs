using SocialAlliance.Models.Twitter;
using SocialAlliance.Models.YouTube;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialAlliance.Models
{
    /// <summary>
    /// The common interface for entries including tweets and videos, allows for sorting by publish date.
    /// </summary>
    public interface ISocialEntry {
        DateTime Published { get; set; }
    }

    /// <summary>
    /// The comparer used for sorting ISocialEntries like tweets and videos by publish date, returning newest entries first.
    /// </summary>
    public class SocialEntriesRecentDateFirstComparer : IComparer<ISocialEntry>
    {
        public int Compare(ISocialEntry x, ISocialEntry y)
        {
            var dateX = x.Published;
            var dateY = y.Published;

            if (dateX > dateY)
            {
                return -1;
            }
            else if (dateX < dateY)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}

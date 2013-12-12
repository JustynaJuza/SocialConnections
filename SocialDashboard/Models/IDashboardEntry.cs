using SocialDashboard.Models.Twitter;
using SocialDashboard.Models.YouTube;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialDashboard.Models
{
    /// <summary>
    /// Common interface for Dashboard entries including Tweets and Videos, allows for sorting by publish date.
    /// </summary>
    public interface IDashboardEntry {
        DateTime Published { get; set; }
    }

    /// <summary>
    /// The Comparer used for sorting IDashboardEntries like Tweets and Videos by publish date, returning newest entries first.
    /// </summary>
    public class DashboardEntriesRecentDateFirstComparer : IComparer<IDashboardEntry>
    {
        public int Compare(IDashboardEntry x, IDashboardEntry y)
        {
            var dateX = x.Published;
            var dateY = y.Published;

            if (dateX > dateY)
            {
                return -1;
            }
            else if (dateX > dateY)
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

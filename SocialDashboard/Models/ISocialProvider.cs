using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialAlliance.Models
{
    public enum AccountType
    {
        youTube,
        twitter,
        facebook
    }

    public interface ISocialProvider
    {
        bool IncludeHowLongSincePublished { get; set; }
    }

    public interface ISocialProviderConfig
    {
        string TimelineId { get; set; }
    }
}

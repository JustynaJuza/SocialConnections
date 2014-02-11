using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialAlliance.Models
{
    /// <summary>
    /// An enum type representing available social account associations.
    /// </summary>
    public enum AccountType
    {
        youTube,
        twitter,
        facebook
    }

    /// <summary>
    /// The common interface for social account API providers, allows publish time description consistentcy in merged timelines.
    /// </summary>
    public interface ISocialProvider
    {
        bool IncludeHowLongSincePublished { get; set; }
    }
}

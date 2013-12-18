using SocialAlliance.Models.WebConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialAlliance.Models
{
    public class TimelineConfigViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Merged { get; set; }
        public bool SingleUser { get; set; }
        public IList<YouTubeProviderConfig> YouTubeProviderConfig { get; set; }
        public IList<TwitterProviderConfig> TwitterProviderConfig { get; set; }

        public TimelineConfigViewModel(TimelineConfig config)
        {
            Id = config.Id;
            Name = config.Name;
            Merged = config.Merged;
            SingleUser = config.SingleUser;
            //YouTubeProviderConfig = config.YouTubeProviders;

        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialDashboard.Models
{
    public class TimelineConfigViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool MergedTimeline { get; set; }
        public IList<TwitterProviderConfigViewModel> TwitterProviderConfig { get; set; }
    }
}
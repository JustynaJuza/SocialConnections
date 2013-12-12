using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialDashboard.Models
{
    public interface IDashboardProvider {
        bool IncludeHowLongSincePublished { get; set; }
    }
}

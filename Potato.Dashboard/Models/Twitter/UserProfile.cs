using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Potato.SocialDashboard.Models.Twitter
{
    public class UserProfile : AbstractExtensions
    {
        public Uri Image { get; set; }
        public Uri ImageInTweet { get; set; }
        public Uri BackgroundImage { get; set; }
        public string BackgroundColor { get; set; }
        public string LinkColor { get; set; }
        public string TextColor { get; set; }
        public string SidebarFillColor { get; set; }
        public string SidebarBorderColor { get; set; }
    }
}
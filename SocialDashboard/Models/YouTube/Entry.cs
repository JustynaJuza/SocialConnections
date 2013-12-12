using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialDashboard.Models.YouTube
{
    public class Entry : AbstractExtensions
    {
        public string Id { get; set; }
        public DateTime Published { get; set; }
        public DateTime? Updated { get; set; }
        public string Title { get; set; }
        public Uri Link { get; set; }
        public Author Author { get; set; }
    }
}
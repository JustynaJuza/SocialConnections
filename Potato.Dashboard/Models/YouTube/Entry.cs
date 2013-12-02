using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Potato.Dashboard.Models.YouTube
{
    public interface IEntry
    {
        string Id { get; set; }
        DateTime? Published { get; set; }
        DateTime? Updated { get; set; }
        string Title { get; set; }
        Uri Link { get; set; }
        Author Author { get; set; }
    }

    public class Entry : AbstractExtensions, IEntry
    {
        public string Id { get; set; }
        public DateTime? Published { get; set; }
        public DateTime? Updated { get; set; }
        public string Title { get; set; }
        public Uri Link { get; set; }
        public Author Author { get; set; }
    }
}
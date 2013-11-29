using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Potato.Dashboard.Models.YouTube
{
    public class Author : AbstractExtensions
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Uri Uri { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Potato.Dashboard.Models.YouTube
{
    public class VideoRating : AbstractExtensions
    {
        public int Likes { get; set; }
        public int Dislikes { get; set; }
    }
}
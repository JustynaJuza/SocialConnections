using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Potato.Dashboard.Models.YouTube
{
    public class ChannelUploads : AbstractExtensions
    {
        public int UploadsCount { get; set; }
        public Uri UploadsFeed { get; set; }
    }
}
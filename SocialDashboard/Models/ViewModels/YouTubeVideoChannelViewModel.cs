using SocialAlliance.Models.YouTube;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialAlliance.Models
{
    public class YouTubeVideoChannelViewModel : AbstractExtensions
    {
        public Channel Channel { get; set; }
        public IList<Playlist> Playlists { get; set; }

        public YouTubeVideoChannelViewModel()
        {
            Playlists = new List<Playlist>();
        }

        public YouTubeVideoChannelViewModel(IList<Playlist> playlists)
        {
            Playlists = playlists;
        }
    }
}
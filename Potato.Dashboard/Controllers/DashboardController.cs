using Potato.Dashboard.Models.YouTube;
using Potato.Dashboard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Potato.Dashboard.Controllers
{
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {
            #region YOUTUBE REQUEST SETTINGS
            var channelUser = "ITV1";
            var playlistTitle = ""; //"Surprise Surprise";
            var videoResultsCount = 5;
            var playlistResultsCount = 3;
            var playlistStartResultsIndex = 1;
            #endregion YOUTUBE REQUEST SETTINGS

            YouTubeVideoChannelViewModel userChannel = new YouTubeVideoChannelViewModel();
            userChannel.Channel = GetYouTubeChannel(channelUser);

            // Show error only if user not found on YouTube.
            if (userChannel.Channel == null)
            {
                TempData["Error"] = "No user with the name " + channelUser + " exists on YouTube.";
                return View();
            }

            // Otherwise, get all playlists for user.
            IList<Playlist> channelPlaylists = GetYouTubeChannelPlaylists(channelUser, playlistTitle, playlistResultsCount, playlistStartResultsIndex, true, videoResultsCount);
            if (channelPlaylists == null)
            {
                // Add error if search for specific playlist returned no results.
                TempData["Error"] = playlistTitle != "" ? "No playlist with the title '" + playlistTitle + "' was found for user " + channelUser : null;
            }
            else
            {
                userChannel.Playlists = new List<Playlist>(channelPlaylists);
            }

            // List user's most recent uploads as first playlist.
            userChannel.Playlists.Insert(0, new Playlist(GetYouTubeUserVideos(channelUser, videoResultsCount))
            {
                Title = "Most Recent Uploads"
            });

            return View(userChannel);
        }

        #region YOUTUBE REQUEST HANDLING
        public Channel GetYouTubeChannel(string userName)
        {
            Uri requestUri = new Uri("http://gdata.youtube.com/feeds/api/users/" + userName + "?alt=json");

            JToken jsonObjectTree = GetJsonRequestResults(requestUri);
            jsonObjectTree = jsonObjectTree != null ? jsonObjectTree.SelectToken("entry") : null;

            if (jsonObjectTree != null)
            {
                Channel requestedChannel = JsonConvert.DeserializeObject<Channel>(jsonObjectTree.ToString());
                return requestedChannel;
            }

            // No entries found for this request.
            return null;
        }

        public IList<Playlist> GetYouTubeChannelPlaylists(string userName, string playlistTitle = "",
            int resultsCount = 50, int startResultsIndex = 1,
            bool withVideos = true, int videoResultsCount = 10, PlaylistOrder orderBy = PlaylistOrder.position)
        {
            // Correct calls exceeding YT handled request results.
            resultsCount = CorrectRequestResultsCount(resultsCount);

            Uri requestUri = new Uri("http://gdata.youtube.com/feeds/api/users/" + userName + "/playlists?alt=json"
                + "&max-results=" + resultsCount + "&start-index=" + startResultsIndex);

            JToken jsonObjectTree = GetJsonRequestResults(requestUri);
            jsonObjectTree = jsonObjectTree != null ? jsonObjectTree.SelectToken("feed").SelectToken("entry") : null;

            if (jsonObjectTree != null)
            {
                List<Playlist> requestedPlaylists = new List<Playlist>();

                // If looking for specific playlist, select entry with correct title.
                if (playlistTitle != "")
                {
                    jsonObjectTree = jsonObjectTree.FirstOrDefault(entry => (string) entry.SelectToken("title").First == playlistTitle);
                    // If no playlist with that title found, fetch and search further playlist results.
                    if (jsonObjectTree == null)
                    {
                        return GetYouTubeChannelPlaylists(userName, playlistTitle, resultsCount, startResultsIndex + resultsCount, withVideos, videoResultsCount, orderBy);
                    }

                    requestedPlaylists.Add(DeserializeJsonPlaylist(jsonObjectTree.ToString(), withVideos, videoResultsCount, orderBy));
                }
                else
                {
                    foreach (var entry in jsonObjectTree)
                    {
                        requestedPlaylists.Add(DeserializeJsonPlaylist(entry.ToString(), withVideos, videoResultsCount, orderBy));
                    }
                }

                return requestedPlaylists;
            }

            // No entries found for this request.
            return null;
        }

        // Video search overload for users.
        public IList<Video> GetYouTubeUserVideos(string userNameOrId,
            int resultsCount = 10, int startResultsIndex = 1, VideoOrder orderBy = VideoOrder.published,
            bool allowRestrictedLocation = false, bool explicitlyEmbeddableOnly = false)
        {
            return GetYouTubeVideos(userNameOrId, false, resultsCount, startResultsIndex, 
                Enum.GetName(typeof(VideoOrder), orderBy), allowRestrictedLocation, explicitlyEmbeddableOnly);

        }

        // Video search overload for playlists.
        public IList<Video> GetYouTubePlaylistVideos(string playlistId, 
            int resultsCount = 10, int startResultsIndex = 1, PlaylistOrder orderBy = PlaylistOrder.position,
            bool allowRestrictedLocation = false, bool explicitlyEmbeddableOnly = false)
        {
            return GetYouTubeVideos(playlistId, true, resultsCount, startResultsIndex, 
                Enum.GetName(typeof(PlaylistOrder), orderBy), allowRestrictedLocation, explicitlyEmbeddableOnly);
        }

        // Base video search method.
        private IList<Video> GetYouTubeVideos(string userOrPlaylistId, bool isPlaylistId = false,
            int resultsCount = 10, int startResultsIndex = 1, string orderBy = "published",
            bool allowRestrictedLocation = false, bool explicitlyEmbeddableOnly = false)
        {
            // Correct calls exceeding YT handled request results.
            resultsCount = CorrectRequestResultsCount(resultsCount);

            // Generate query string.
            // Important! YT will search live up-to-date database with base string, but including additional query keywords will result in:
            // searching cached content => removing some info about actual likes/dislikes, views and truncating descriptions.
            Uri requestUri = new Uri("http://gdata.youtube.com/feeds/api/"
                + (isPlaylistId ? "playlists/" + userOrPlaylistId : "users/" + userOrPlaylistId + "/uploads") + "?alt=json"
                + "&max-results=" + resultsCount + "&start-index=" + startResultsIndex // live data search up to this line
                + (orderBy != "published" ? "&orderby=" + orderBy : "")
                + (explicitlyEmbeddableOnly ? "&format=5" : "")
                + (allowRestrictedLocation ? "&safeSearch=strict" : ""));

            JToken jsonObjectTree = GetJsonRequestResults(requestUri);
            jsonObjectTree = jsonObjectTree != null ? jsonObjectTree.SelectToken("feed").SelectToken("entry") : null;

            if (jsonObjectTree != null)
            {
                List<Video> requestedVideos = new List<Video>();
                foreach (var entry in jsonObjectTree)
                {
                    requestedVideos.Add(JsonConvert.DeserializeObject<Video>(entry.ToString()));
                }

                return requestedVideos;
            }

            // No entries found for this request.
            return null;
        }

        #endregion YOUTUBE REQUEST HANDLING

        #region HELPERS
        private JToken GetJsonRequestResults(Uri requestUri)
        {
            WebClient requestHandler = new WebClient();
            requestHandler.Headers.Add("GData-Version: 2");
            try
            {
                var jsonString = requestHandler.DownloadString(requestUri);
                return JObject.Parse(jsonString);
                // SelectToken returns child values only! .First/Last return child object with both key+value;
            }
            catch (System.Net.WebException ex)
            {
                System.Diagnostics.Debug.WriteLine("WebRequest Error: " + ex.Message);
                return null;
            }
        }

        private int CorrectRequestResultsCount(int resultsCount)
        {
            if (resultsCount > 50)
            {
                System.Diagnostics.Debug.WriteLine("Error: YouTube can only handle requests for up to 50 results.");
                return 50;
            }

            // Number of request results is correct.
            return resultsCount;
        }

        private Playlist DeserializeJsonPlaylist(string jsonObjectString, bool withVideos, int videoResultsCount, PlaylistOrder orderBy)
        {
            var requestedPlaylist = JsonConvert.DeserializeObject<Playlist>(jsonObjectString);
            // Include playlist videos.
            if (withVideos)
            {
                requestedPlaylist.Entries = GetYouTubePlaylistVideos(requestedPlaylist.Id, videoResultsCount, 1, orderBy);
            }
            return requestedPlaylist;
        }
        #endregion HELPERS
    }
}
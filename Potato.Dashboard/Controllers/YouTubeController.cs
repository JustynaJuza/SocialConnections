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
    public class YouTubeController : Controller
    {
        public ActionResult Index()
        {
            #region YOUTUBE REQUEST SETTINGS
            // YouTube user whose videos we request.
            string youTubeUser = "ITV1";
            // Playlist name for fetching videos from specific playlist.
            string playlistTitle = ""; //"Surprise Surprise";
            // How many results should be feteched for user/playlist videos.
            int videoResultsCount = 5;
            // How many results should be feteched for playlists.
            int playlistResultsCount = 3;
            // Index of the first playlist to be fetched (starting with 1).
            int playlistStartResultsIndex = 1;
            // Index of the first playlist to be fetched (starting with 1).
            int userVideosStartResultsIndex = 1;
            // The ordering of videos in a playlist.
            // Important: Live database search (with updated likes, views, etc.) only with 'position'.
            PlaylistOrder playlistVideosOrder = PlaylistOrder.position;
            // The ordering of videos requested from a specific user's uploads.
            // Important: Live database search (with updated likes, views, etc.) only with 'relevance'.
            VideoOrder userVideosOrder = VideoOrder.published;
            // Include a word description of when the video was published.
            bool calculateHowLongSincePublished = true;
            ViewBag.HowLongSincePublished = calculateHowLongSincePublished;
            #endregion YOUTUBE REQUEST SETTINGS

            var userChannel = new YouTubeVideoChannelViewModel();
            userChannel.Channel = GetYouTubeChannel(youTubeUser);

            // Show error only if user not found on YouTube.
            if (userChannel.Channel == null)
            {
                TempData["Error"] = "No user with the name " + youTubeUser + " exists on YouTube.";
                return View();
            }

            // Get all playlists for user
            if (playlistResultsCount > 0)
            {
                var channelPlaylists = GetYouTubeChannelPlaylists(youTubeUser, playlistTitle, calculateHowLongSincePublished, playlistResultsCount, playlistStartResultsIndex, true, videoResultsCount, playlistVideosOrder);
                if (channelPlaylists == null)
                {
                    // Add error if search for specific playlist returned no results.
                    TempData["Error"] = playlistTitle != "" ? "No playlist with the title '" + playlistTitle + "' was found for user " + youTubeUser : null;
                }
                else
                {
                    userChannel.Playlists = new List<Playlist>(channelPlaylists);
                }
            }

            // List user's most recent uploads as first playlist.
            userChannel.Playlists.Insert(0, new Playlist(GetYouTubeUserVideos(youTubeUser, calculateHowLongSincePublished, videoResultsCount, userVideosStartResultsIndex, userVideosOrder))
            {
                Title = "Most Recent Uploads",
                Link = new Uri("http://www.youtube.com/user/" + youTubeUser + "/videos")
            });

            return View(userChannel);
        }

        #region YOUTUBE REQUEST HANDLING
        public Channel GetYouTubeChannel(string userName)
        {
            var requestUri = "users/" + userName + "?alt=json";

            var jsonObject = GetJsonRequestResults(requestUri);
            jsonObject = jsonObject != null ? jsonObject.SelectToken("entry") : null;
            if (jsonObject != null)
            {
                var requestedChannel = JsonConvert.DeserializeObject<Channel>(jsonObject.ToString());
                return requestedChannel;
            }

            // No entries found for this request.
            return null;
        }

        public IList<Playlist> GetYouTubeChannelPlaylists(string userName, string playlistTitle = "",
            bool calculateHowLongSincePublished = false, int resultsCount = 50, int startResultsIndex = 1,
            bool withVideos = true, int videoResultsCount = 10, PlaylistOrder orderBy = PlaylistOrder.position)
        {
            // Correct calls exceeding YT handled request results.
            resultsCount = CorrectRequestResultsCount(resultsCount);

            var requestUri = "users/" + userName + "/playlists?alt=json"
                + "&max-results=" + resultsCount + "&start-index=" + startResultsIndex;

            var jsonObject = GetJsonRequestResults(requestUri);
            jsonObject = jsonObject != null ? jsonObject.SelectToken("feed").SelectToken("entry") : null;
            if (jsonObject != null)
            {
                var requestedPlaylists = new List<Playlist>();

                // If looking for specific playlist, select entry with correct title.
                if (playlistTitle != "")
                {
                    jsonObject = jsonObject.FirstOrDefault(entry => (string) entry.SelectToken("title").First == playlistTitle);
                    // If no playlist with that title found, fetch and search further playlist results.
                    if (jsonObject == null)
                    {
                        return GetYouTubeChannelPlaylists(userName, playlistTitle, calculateHowLongSincePublished, resultsCount, startResultsIndex + resultsCount, withVideos, videoResultsCount, orderBy);
                    }

                    requestedPlaylists.Add(DeserializeJsonPlaylist(jsonObject.ToString(), withVideos, calculateHowLongSincePublished, videoResultsCount, orderBy));
                }
                else
                {
                    foreach (var entry in jsonObject)
                    {
                        requestedPlaylists.Add(DeserializeJsonPlaylist(entry.ToString(), withVideos, calculateHowLongSincePublished, videoResultsCount, orderBy));
                    }
                }

                return requestedPlaylists;
            }

            // No entries found for this request.
            return null;
        }

        // Video search overload for users.
        public IList<Video> GetYouTubeUserVideos(string userNameOrId, bool calculateHowLongSincePublished = false,
            int resultsCount = 10, int startResultsIndex = 1, VideoOrder orderBy = VideoOrder.relevance,
            bool allowRestrictedLocation = false, bool explicitlyEmbeddableOnly = false)
        {
            return GetYouTubeVideos(userNameOrId, false, calculateHowLongSincePublished, resultsCount, startResultsIndex, 
                Enum.GetName(typeof(VideoOrder), orderBy), allowRestrictedLocation, explicitlyEmbeddableOnly);

        }

        // Video search overload for playlists.
        public IList<Video> GetYouTubePlaylistVideos(string playlistId, bool calculateHowLongSincePublished = false, 
            int resultsCount = 10, int startResultsIndex = 1, PlaylistOrder orderBy = PlaylistOrder.position,
            bool allowRestrictedLocation = false, bool explicitlyEmbeddableOnly = false)
        {
            return GetYouTubeVideos(playlistId, true, calculateHowLongSincePublished, resultsCount, startResultsIndex, 
                Enum.GetName(typeof(PlaylistOrder), orderBy), allowRestrictedLocation, explicitlyEmbeddableOnly);
        }

        // Base video search method.
        private IList<Video> GetYouTubeVideos(string userOrPlaylistId, bool isPlaylistId = false, bool calculateHowLongSincePublished = false,
            int resultsCount = 10, int startResultsIndex = 1, string orderBy = "published",
            bool allowRestrictedLocation = false, bool explicitlyEmbeddableOnly = false)
        {
            // Correct calls exceeding YT handled request results.
            resultsCount = CorrectRequestResultsCount(resultsCount);

            // Generate query string.
            // Important! YT will search live up-to-date database with base string, but including additional query keywords will result in:
            // searching cached content => removing some info about actual likes/dislikes, views and truncating descriptions.
            var requestUri = (isPlaylistId ? "playlists/" + userOrPlaylistId : "users/" + userOrPlaylistId + "/uploads") + "?alt=json"
                + "&max-results=" + resultsCount + "&start-index=" + startResultsIndex // live data search up to this line
                + (orderBy != "relevance" && orderBy != "position" ? "&orderby=" + orderBy : "")
                + (explicitlyEmbeddableOnly ? "&format=5" : "")
                + (allowRestrictedLocation ? "&safeSearch=strict" : "");

            var jsonObject = GetJsonRequestResults(requestUri);
            jsonObject = jsonObject != null ? jsonObject.SelectToken("feed").SelectToken("entry") : null;
            if (jsonObject != null)
            {
                var requestedVideos = new List<Video>();
                if (calculateHowLongSincePublished)
                {
                    // Include a string for readable displaying how long ago the entry was published.
                    foreach (var entry in jsonObject)
                    {
                        var requestedVideo = JsonConvert.DeserializeObject<Video>(entry.ToString());
                        requestedVideo.calculateHowLongSincePublished();
                        requestedVideos.Add(requestedVideo);
                    }
                }
                else
                {
                    foreach (var entry in jsonObject)
                    {
                        requestedVideos.Add(JsonConvert.DeserializeObject<Video>(entry.ToString()));
                    }
                }

                return requestedVideos;
            }

            // No entries found for this request.
            return null;
        }
        #endregion YOUTUBE REQUEST HANDLING

        #region HELPERS
        private JToken GetJsonRequestResults(string requestUri)
        {
            var requestHandler = new WebClient();
            requestHandler.BaseAddress = "http://gdata.youtube.com/feeds/api/";
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

        private Playlist DeserializeJsonPlaylist(string jsonObjectString, bool withVideos, bool calculateHowLongSincePublished, int videoResultsCount, PlaylistOrder orderBy)
        {
            var requestedPlaylist = JsonConvert.DeserializeObject<Playlist>(jsonObjectString);
            // Include playlist videos.
            if (withVideos)
            {
                requestedPlaylist.Entries = GetYouTubePlaylistVideos(requestedPlaylist.Id, calculateHowLongSincePublished, videoResultsCount, 1, orderBy);
            }
            return requestedPlaylist;
        }
        #endregion HELPERS
    }
}
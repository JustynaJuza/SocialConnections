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
            var channelUser = "paulsoaresjr";

            // Get all playlists for user.
            IList<Playlist> channelPlaylists = GetYouTubeChannelPlaylists(channelUser, 15);
            // Loading all entries for all selected playlists.
            //foreach (var playlist in channelPlaylists)
            //{
            //    playlist.Entries = GetYouTubeVideos(playlist.Id, true, 10);
            //}

            YouTubeVideoChannelViewModel userChannel = new YouTubeVideoChannelViewModel();
            userChannel.Channel = GetYouTubeChannel(channelUser);
            userChannel.Playlists = new List<Playlist>(channelPlaylists);
            // List user's most recent uploads.
            userChannel.Playlists.Add(new Playlist(GetYouTubeVideos(channelUser, false, 10))
            {
                Title = "Most Recent Uploads"
            });

            return View(userChannel);
        }

        public IList<Video> GetYouTubeVideos(string userOrPlaylistId, bool isPlaylistId = false, int resultsCount = 50, EntryOrder orderBy = EntryOrder.published, bool allowRestrictedLocation = false, bool explicitlyEmbeddableOnly = false)
        {
            // Correct calls exceeding YT handled request results.
            if (resultsCount > 50)
            {
                resultsCount = 50;
                System.Diagnostics.Debug.WriteLine("Error: YouTube can only handle requests for up to 50 results.");
            }

            // Generate query string.
            // Important! YT will search live up-to-date database with base string, but including additional query keywords will result in:
            // searching cached content => removing some info about actual likes/dislikes, views and truncating descriptions.
            Uri requestUri = new Uri("http://gdata.youtube.com/feeds/api/"
                + (isPlaylistId ? "playlists/" + userOrPlaylistId : "users/" + userOrPlaylistId + "/uploads") + "?alt=json&max-results=" + resultsCount // live data search only with this line
                + (orderBy != EntryOrder.published ? "&orderby=" + Enum.GetName(typeof(EntryOrder), orderBy) : "")
                + (explicitlyEmbeddableOnly ? "&format=5" : "")
                + (allowRestrictedLocation ? "&safeSearch=strict" : ""));

            //System.Diagnostics.Debug.WriteLine(requestUri);

            WebClient requestHandler = new WebClient();
            requestHandler.Headers.Add("GData-Version: 2");
            var jsonString = requestHandler.DownloadString(requestUri);

            JToken jsonObjectTree = JObject.Parse(jsonString);
            jsonObjectTree = jsonObjectTree.SelectToken("feed").SelectToken("entry"); //.Where(entry => entry.SelectToken("app$control") != null && entry.SelectToken("app$draft") != null); // SelectToken returns child values only! .First/Last return child object with both key+value;
            //JArray allRequestEntries = JArray.Parse(jsonObjectTree.Values());

            List<Video> requestedVideos = new List<Video>();
            foreach (var entry in jsonObjectTree)
            {
                    requestedVideos.Add(JsonConvert.DeserializeObject<Video>(entry.ToString()));
            }

            return requestedVideos;
        }

        public Channel GetYouTubeChannel(string userName)
        {
            Uri requestUri = new Uri("http://gdata.youtube.com/feeds/api/users/" + userName + "?alt=json");

            WebClient requestHandler = new WebClient();
            requestHandler.Headers.Add("GData-Version: 2");
            var jsonString = requestHandler.DownloadString(requestUri);

            JToken jsonObjectTree = JObject.Parse(jsonString);
            jsonObjectTree = jsonObjectTree.SelectToken("entry");

            Channel requestedChannel = JsonConvert.DeserializeObject<Channel>(jsonObjectTree.ToString());
            return requestedChannel;
        }

        public IList<Playlist> GetYouTubeChannelPlaylists(string userName, int resultsCount = 50)
        {
            // Correct calls exceeding YT handled request results.
            if (resultsCount > 50)
            {
                resultsCount = 50;
                System.Diagnostics.Debug.WriteLine("Error: YouTube can only handle requests for up to 50 results.");
            }

            Uri requestUri = new Uri("http://gdata.youtube.com/feeds/api/users/" + userName + "/playlists?alt=json" + "&max-results=" + resultsCount);

            WebClient requestHandler = new WebClient();
            requestHandler.Headers.Add("GData-Version: 2");
            var jsonString = requestHandler.DownloadString(requestUri);

            JToken jsonObjectTree = JObject.Parse(jsonString);
            jsonObjectTree = jsonObjectTree.SelectToken("feed").SelectToken("entry");

            List<Playlist> requestedPlaylists = new List<Playlist>();
            foreach (var entry in jsonObjectTree)
            {
                requestedPlaylists.Add(JsonConvert.DeserializeObject<Playlist>(entry.ToString()));
            }

            return requestedPlaylists;
        }        
    }
}
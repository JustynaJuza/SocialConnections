using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocialAlliance.Models.WebConfig;
using SocialAlliance.Models.YouTube;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Web;

namespace SocialAlliance.Models
{
    /// <summary>
    /// Provides YouTube requests and response handling for channels, videos and playlists.
    /// </summary>
    public class YouTubeProvider : AbstractExtensions, ISocialProvider
    {
        #region YOUTUBE REQUEST SETTINGS
        /// <summary>
        /// YouTube user whose videos we request.</summary>
        public string User { get; set; }
        /// <summary>
        /// Playlist name for fetching videos from specific playlist.</summary>
        public string PlaylistTitle { get; set; }
        /// <summary>
        /// How many results should be feteched for user/playlistId videos.</summary>
        public int VideoResultsCount { get; set; }
        /// <summary>
        /// How many results should be feteched for playlists.</summary>
        public int PlaylistResultsCount { get; set; }
        /// <summary>
        /// Include videos in found playlists, based on VideoResultsCount and VideosStartResultsIndex. </summary>
        public bool IncludePlaylistVideos { get; set; }
        /// <summary>
        /// Index of the first video to be fetched (starting with 1).</summary>
        public int VideosStartResultsIndex { get; set; }
        /// <summary>
        /// Index of the first playlist to be fetched (starting with 1).</summary>
        public int PlaylistsStartResultsIndex { get; set; }
        /// <summary>
        /// The ordering of videos requested from a specific user's uploads.
        /// <para>Important: Live database search (with updated likes, views, etc.) only with 'relevance'.</para></summary>
        public VideoOrder UserVideosOrder { get; set; }
        /// <summary>
        /// The ordering of videos in a playlist.
        /// <para>Important: Live database search (with updated likes, views, etc.) only with 'position'.</para></summary>
        public PlaylistOrder PlaylistVideosOrder { get; set; }
        /// <summary>
        /// Include a word description of when the video was published.</summary>
        public bool IncludeHowLongSincePublished { get; set; }
        #endregion YOUTUBE REQUEST SETTINGS

        #region CONSTRUCTORS
        private YouTubeProvider()
        {
            // Default settings.
            VideoResultsCount = 10;
            PlaylistResultsCount = 0;
            VideosStartResultsIndex = 1;
            PlaylistsStartResultsIndex = 1;
            IncludePlaylistVideos = true;
            UserVideosOrder = VideoOrder.published;
            PlaylistVideosOrder = PlaylistOrder.position;
            IncludeHowLongSincePublished = false;
        }

        /// <summary>
        /// Provides requests for the YouTube user's videos with default settings.
        /// </summary>
        /// <param name="youTubeUser">The YouTube user's username.</param>
        public YouTubeProvider(string youTubeUser)
            : this()
        {
            User = youTubeUser;
        }

        /// <summary>
        /// Provides requests for the YouTube user's videos with an initially assigned playlist title to search for.
        /// </summary>
        /// <param name="youTubeUser">The YouTube user's username.</param>
        /// <param name="playlistTitle">The title of the requested playlist.</param>
        /// <param name="includePlaylistVideos">Request the videos of this playlist.</param>
        public YouTubeProvider(string youTubeUser, string playlistTitle = "", bool includePlaylistVideos = true)
            : this(youTubeUser)
        {
            PlaylistTitle = playlistTitle;
            IncludePlaylistVideos = includePlaylistVideos;
        }

        /// <summary>
        /// Provides requests for the YouTube user's videos with a non-default video count or order to search for.
        /// </summary>
        /// <param name="youTubeUser">The YouTube user's username.</param>
        /// <param name="videoResultsCount">The number of videos returned in request.</param>
        /// <param name="userVideosOrder">The 'orderby' value by which videos are sorted.</param>
        public YouTubeProvider(string youTubeUser, int videoResultsCount = 10, VideoOrder userVideosOrder = VideoOrder.published)
            : this(youTubeUser)
        {
            VideoResultsCount = videoResultsCount;
            UserVideosOrder = userVideosOrder;
        }

        public YouTubeProvider(YouTubeProviderConfig config)
        {
            User = config.User;
            PlaylistTitle = config.PlaylistTitle;
            VideoResultsCount = config.VideoResultsCount;
            PlaylistResultsCount = config.PlaylistResultsCount;
            VideosStartResultsIndex = config.VideosStartResultsIndex;
            PlaylistsStartResultsIndex = config.PlaylistsStartResultsIndex;
            IncludePlaylistVideos = config.IncludePlaylistVideos;
            UserVideosOrder = config.UserVideosOrder;
            PlaylistVideosOrder = config.PlaylistVideosOrder;
            IncludeHowLongSincePublished = config.IncludeHowLongSincePublished;
        }
        #endregion CONSTRUCTORS

        /// <summary>
        /// Returns YouTube channel information and user's uploaded videos based on instance settings.
        /// <para>Also includes additional playlist results if specified by instance settings.</para>
        /// </summary>
        /// <param name="errorText">Error text if request failed, with purpose for a TempData["Error"] to be assigned.</param>
        public YouTubeVideoChannelViewModel GetYouTubeUserData(out string errorText)
        {
            errorText = null;
            var userChannel = new YouTubeVideoChannelViewModel();
            userChannel.Channel = GetChannel(User);

            // Set error only if user not found on YouTube.
            if (userChannel.Channel == null)
            {
                errorText = "No user with the name " + User + " exists on YouTube.";
                return null;
            }

            // Get all playlists for user
            if (PlaylistResultsCount > 0)
            {
                var channelPlaylists = GetChannelPlaylists(User, PlaylistTitle, IncludeHowLongSincePublished, PlaylistResultsCount, PlaylistsStartResultsIndex, true, VideoResultsCount, PlaylistVideosOrder);
                if (channelPlaylists == null)
                {
                    // Add error if search for specific playlist returned no results.
                    errorText = PlaylistTitle != "" ? "No playlist with the title '" + PlaylistTitle + "' was found for user " + User : null;
                }
                else
                {
                    userChannel.Playlists = new List<Playlist>(channelPlaylists);
                }
            }

            // List user's most recent uploads as first playlist.
            userChannel.Playlists.Insert(0, new Playlist(GetChannelVideos(User, IncludeHowLongSincePublished, VideoResultsCount, VideosStartResultsIndex, UserVideosOrder))
            {
                Title = "Most Recent Uploads",
                Link = new Uri("http://www.youtube.com/user/" + User + "/videos")
            });

            return userChannel;
        }

        public Channel GetChannel()
        {
            return GetChannel(User);
        }

        public IList<Playlist> GetChannelPlaylists(bool withVideos, string playlistTitle = "")
        {
            return GetChannelPlaylists(User, playlistTitle, IncludeHowLongSincePublished,
                PlaylistResultsCount, PlaylistsStartResultsIndex,
                withVideos, VideoResultsCount, PlaylistVideosOrder);
        }

        public IList<Video> GetChannelVideos()
        {
            return GetChannelVideos(User, IncludeHowLongSincePublished,
                VideoResultsCount, VideosStartResultsIndex, UserVideosOrder);
        }

        #region YOUTUBE REQUEST HANDLING
        public Video GetVideo(string id, bool includeHowLongSincePublished = false)
        {
            var requestUri = "videos/" + id + "?alt=json";

            var jsonObject = GetJsonRequestResults(requestUri);
            jsonObject = jsonObject != null ? jsonObject.SelectToken("entry") : null;
            if (jsonObject != null)
            {
                var requestedVideo = JsonConvert.DeserializeObject<Video>(jsonObject.ToString());
                if (includeHowLongSincePublished)
                {
                    requestedVideo.IncludeHowLongSincePublished();
                }
                return requestedVideo;
            }

            // No entries found for this request.
            return null;
        }

        public Channel GetChannel(string userName)
        {
            var requestUri = "users/" + userName + "?alt=json";

            var jsonObject = GetJsonRequestResults(requestUri);
            jsonObject = jsonObject != null ? jsonObject.SelectToken("entry") : null;
            if (jsonObject != null)
            {
                return JsonConvert.DeserializeObject<Channel>(jsonObject.ToString());
            }

            // No entries found for this request.
            return null;
        }

        public IList<Playlist> GetChannelPlaylists(string userName, string playlistTitle = "",
            bool includeHowLongSincePublished = false, int resultsCount = 50, int startResultsIndex = 1,
            bool withVideos = true, int videoResultsCount = 10, PlaylistOrder orderBy = PlaylistOrder.position)
        {
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
                        return GetChannelPlaylists(userName, playlistTitle, includeHowLongSincePublished, resultsCount, startResultsIndex + resultsCount, withVideos, videoResultsCount, orderBy);
                    }

                    requestedPlaylists.Add(DeserializeJsonPlaylist(jsonObject.ToString(), withVideos, includeHowLongSincePublished, videoResultsCount, orderBy));
                }
                else
                {
                    foreach (var entry in jsonObject)
                    {
                        requestedPlaylists.Add(DeserializeJsonPlaylist(entry.ToString(), withVideos, includeHowLongSincePublished, videoResultsCount, orderBy));
                    }
                }

                return requestedPlaylists;
            }

            // No entries found for this request.
            return new List<Playlist>();
        }

        // Video search overload for users.
        public IList<Video> GetChannelVideos(string userNameOrId, bool includeHowLongSincePublished = false,
            int resultsCount = 10, int startResultsIndex = 1, VideoOrder orderBy = VideoOrder.relevance,
            bool allowRestrictedLocation = false, bool explicitlyEmbeddableOnly = false)
        {
            return GetVideos(userNameOrId, false, includeHowLongSincePublished, resultsCount, startResultsIndex,
                Enum.GetName(typeof(VideoOrder), orderBy), allowRestrictedLocation, explicitlyEmbeddableOnly);

        }

        // Video search overload for playlists.
        public IList<Video> GetPlaylistVideos(string playlistId, bool includeHowLongSincePublished = false,
            int resultsCount = 10, int startResultsIndex = 1, PlaylistOrder orderBy = PlaylistOrder.position,
            bool allowRestrictedLocation = false, bool explicitlyEmbeddableOnly = false)
        {
            return GetVideos(playlistId, true, includeHowLongSincePublished, resultsCount, startResultsIndex,
                Enum.GetName(typeof(PlaylistOrder), orderBy), allowRestrictedLocation, explicitlyEmbeddableOnly);
        }

        // Base video search method.
        private IList<Video> GetVideos(string userOrPlaylistId, bool isPlaylistId = false, bool includeHowLongSincePublished = false,
            int resultsCount = 10, int startResultsIndex = 1, string orderBy = "published",
            bool allowRestrictedLocation = false, bool explicitlyEmbeddableOnly = false)
        {
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
                if (includeHowLongSincePublished)
                {
                    foreach (var entry in jsonObject)
                    {
                        var requestedVideo = JsonConvert.DeserializeObject<Video>(entry.ToString());
                        requestedVideo.IncludeHowLongSincePublished();
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
            return new List<Video>();
        }
        #endregion YOUTUBE REQUEST HANDLING

        #region HELPERS
        /// <summary>
        /// Handles YouTube requests with the specified requestUri, returning a JsonObject.
        /// </summary>
        /// <param name="requestUri">The request Uri with query parameters.</param>
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

        /// <summary>
        /// Correct calls exceeding YouTube handled request results. Returns maximum value (50) if exceeded.
        /// </summary>
        /// <param name="resultsCount">Number of currently expected results.</param>
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

        /// <summary>
        /// Returns a Playlist object with or without videos, depending on YouTube request settings.
        /// </summary>
        private Playlist DeserializeJsonPlaylist(string jsonObjectString, bool withVideos, bool includeHowLongSincePublished, int videoResultsCount, PlaylistOrder orderBy)
        {
            var requestedPlaylist = JsonConvert.DeserializeObject<Playlist>(jsonObjectString);
            // Include playlist videos.
            if (withVideos)
            {
                requestedPlaylist.Entries = GetPlaylistVideos(requestedPlaylist.Id, includeHowLongSincePublished, videoResultsCount, 1, orderBy);
            }
            return requestedPlaylist;
        }
        #endregion HELPERS
    }
}
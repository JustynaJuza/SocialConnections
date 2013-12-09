using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace SocialDashboard.Models.YouTube
{
    public enum PlaylistOrder
    {
        position,
        published,
        viewCount,
        commentCount,
        duration,
        title
    }

    [JsonConverter(typeof(JsonYouTubePlaylistConverter))]
    public class Playlist : Entry
    {
        public IList<Video> Entries { get; set; }
        public int EntriesCount { get; set; }
        public Uri EntriesFeed { get; set; }

        public Playlist() {
            Entries = new List<Video>();
        }
        public Playlist(IList<Video> videoList)
        {
            Entries = videoList;
            EntriesCount = videoList.Count;
        }
    }

    public class JsonYouTubePlaylistConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Serializing to JSON was not needed and has not yet been implemented.");
        }

        // Deserialize - JSON to C# object mapping.
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);

            // Populate C# object with according JObject data.
            return new Playlist()
            {
                Id = Regex.Replace((string) jsonObject.SelectToken("id").First, "tag:youtube.com,....:user:.*:playlist:", ""),
                Published = (DateTime) jsonObject.SelectToken("published").First,
                Updated = (DateTime) jsonObject.SelectToken("updated").First,
                Title = (string) jsonObject.SelectToken("title").First,
                Link = new Uri((string) jsonObject.SelectToken("link").ElementAt(1).SelectToken("href")),
                Author = new Author()
                {
                    Name = (string) jsonObject.SelectToken("author").First.SelectToken("name").First,
                    Uri = new Uri((string) jsonObject.SelectToken("author").First.SelectToken("uri").First),
                    Id = (string) jsonObject.SelectToken("author").First.SelectToken("yt$userId").First
                },
                EntriesCount = (int) jsonObject.SelectToken("yt$countHint").First,
            };
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Playlist).IsAssignableFrom(objectType);
        }
    }
}
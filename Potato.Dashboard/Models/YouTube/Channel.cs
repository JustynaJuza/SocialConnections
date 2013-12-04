using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Potato.Dashboard.Models.YouTube
{
    [JsonConverter(typeof(JsonYouTubeChannelConverter))]
    public class Channel : Entry
    {
        public Uri Thumbnail { get; set; }
        public ChannelStatistics Statistics { get; set; }
        public ChannelUploads Uploads { get; set; }
    }

    public class JsonYouTubeChannelConverter : JsonConverter
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
            return new Channel()
            {
                Id = Regex.Replace((string) jsonObject.SelectToken("id").First, "tag:youtube.com,....:user:", ""),
                Published = (DateTime) jsonObject.SelectToken("published").First,
                Updated = (DateTime) jsonObject.SelectToken("updated").First,
                Title = (string) jsonObject.SelectToken("title").First,
                Link = new Uri((string) jsonObject.SelectToken("link").First.SelectToken("href")),
                Author = new Author()
                {
                    Name = (string) jsonObject.SelectToken("author").First.SelectToken("name").First,
                    Uri = new Uri((string) jsonObject.SelectToken("author").First.SelectToken("uri").First),
                    Id = (string) jsonObject.SelectToken("author").First.SelectToken("yt$userId").First
                },
                Thumbnail = new Uri((string) jsonObject.SelectToken("media$thumbnail").First),
                Uploads = new ChannelUploads()
                {
                    UploadsCount = (int) jsonObject.SelectToken("gd$feedLink").ElementAt(6).SelectToken("countHint"),
                    UploadsFeed = new Uri((string) jsonObject.SelectToken("gd$feedLink").ElementAt(6).SelectToken("href"))
                },
                Statistics = new ChannelStatistics()
                {
                    SubscriberCount = (int) jsonObject.SelectToken("yt$statistics").SelectToken("subscriberCount"),
                    TotalUploadViews = (int) jsonObject.SelectToken("yt$statistics").SelectToken("totalUploadViews")
                }
            };
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Channel).IsAssignableFrom(objectType);
        }
    }
}
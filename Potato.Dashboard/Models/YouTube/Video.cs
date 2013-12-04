using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Potato.Dashboard.Models.YouTube
{
    public enum VideoOrder
    {
        relevance,
        published,
        viewCount
    }

    [JsonConverter(typeof(JsonYouTubeVideoConverter))]
    public class Video : Entry
    {
        public VideoComments Comments { get; set; }
        public VideoStatistics Statistics { get; set; }
        public VideoRating Rating { get; set; }
        public VideoMedia Media { get; set; }
        public string HowLongSincePublished { get; set; }

        public void calculateHowLongSincePublished()
        {
            int timeDifference;
            if (DateTime.Now.Year - Published.Value.Year > 0 && (DateTime.Now - Published.Value).TotalDays > 365)
            {
                HowLongSincePublished = "> a year ago";
            }
            else if ((timeDifference = DateTime.Now.Month - Published.Value.Month) > 0)
            {
                HowLongSincePublished =  timeDifference == 1 ? "last month" : timeDifference.ToString() + " months ago";
            }
            else if ((timeDifference = DateTime.Now.Day - Published.Value.Day) > 0)
            {
                HowLongSincePublished = timeDifference.ToString() + (timeDifference == 1 ? " day" : " days") + " ago";
            }
            else if ((timeDifference = DateTime.Now.Hour - Published.Value.Hour) > 0)
            {
                HowLongSincePublished = timeDifference.ToString() + (timeDifference == 1 ? " hour" : " hours") + " ago";
            }
            else if ((timeDifference = DateTime.Now.Minute - Published.Value.Minute) > 0)
            {
                HowLongSincePublished = timeDifference.ToString() + (timeDifference == 1 ? " min" : " mins") + " ago";
            }
            else
            {
                HowLongSincePublished = "< 1 min ago";
            }
        }
    }

    public class JsonYouTubeVideoConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Serializing to JSON was not needed and has not yet been implemented.");
        }

        // Deserialize - JSON to C# object mapping.
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            //System.Diagnostics.Debug.WriteLine(jsonObject);

            // Populate C# object with according JObject data.
            var deserializedVideo = new Video()
            {
                Id = (string) jsonObject.SelectToken("id").First,
                Published = (DateTime) jsonObject.SelectToken("published").First,
                Updated = (DateTime) jsonObject.SelectToken("updated").First,
                Title = (string) jsonObject.SelectToken("title").First,
                Link = new Uri((string) jsonObject.SelectToken("link").First.SelectToken("href")),
                Author = new Author()
                {
                    Name = (string) jsonObject.SelectToken("author").First.SelectToken("name").First,
                    Uri = new Uri(((string) jsonObject.SelectToken("author").First.SelectToken("uri").First).Replace("&feature=youtube_gdata", "")),
                    Id = (string) jsonObject.SelectToken("author").First.SelectToken("yt$userId").First
                }
            };

            // Reformat Id according to how video was retrieved.
            if (Regex.IsMatch(deserializedVideo.Id, "video"))
            {
                deserializedVideo.Id = Regex.Replace(deserializedVideo.Id, "tag:youtube.com,....:video:", "");
            }
            else
            {
                deserializedVideo.Id = deserializedVideo.Link.OriginalString.Replace("http://www.youtube.com/watch?v=", "").Replace("&feature=youtube_gdata", "");
            }

            // Check if video is playable, not saved as draft or inaccessible, because YouTube queries happen to be faulty sometimes and return videos they should not.
            if (jsonObject.SelectToken("media$group").SelectToken("media$content") != null)
            {
                var jsonSubObject = jsonObject.SelectToken("media$group");
                deserializedVideo.Media = new VideoMedia()
                {
                    Title = (string) jsonSubObject.SelectToken("media$title").First,
                    Category = (string) jsonSubObject.SelectToken("media$category").First.SelectToken("label"),
                    Description = (string) jsonSubObject.SelectToken("media$description").First,
                    // TODO: Use SerializerOptions or Enum to allow selecting between thumbnail sizes.
                    Thumbnail = new Uri((string) jsonSubObject.SelectToken("media$thumbnail").First(thumbnail => (string) thumbnail.SelectToken("yt$name") == "mqdefault").SelectToken("url")),
                    Content = new VideoMediaContent()
                    {
                        Duration = (int) jsonSubObject.SelectToken("media$content").First.SelectToken("duration"),
                        ContentType = (string) jsonSubObject.SelectToken("media$content").First.SelectToken("type"),
                        Source = new Uri(((string) jsonSubObject.SelectToken("media$content").First.SelectToken("url")).Replace("&app=youtube_gdata", "")),
                    }
                };
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Video (ID: " + deserializedVideo.Id + ") " + deserializedVideo.Title + " is unavalable, status: " + (string) jsonObject.SelectToken("app$control").SelectToken("yt$state").SelectToken("name"));
            }

            // Include comments, statistics and rating if available (unavailable on new videos not yet rated or cached)
            // YouTube introducing changes in comment system in November 2013, integrating Google+ - to be reviewed.
            if (jsonObject.SelectToken("gd$comments") != null)
            {
                var jsonSubObject = jsonObject.SelectToken("gd$comments").SelectToken("gd$feedLink");
                deserializedVideo.Comments = new VideoComments()
                {
                    CommentsCount = (int) jsonSubObject.SelectToken("countHint"),
                    CommentsFeed = new Uri((string) jsonSubObject.SelectToken("href"))
                };
            }
            if (jsonObject.SelectToken("yt$statistics") != null)
            {
                var jsonSubObject = jsonObject.SelectToken("yt$statistics");
                deserializedVideo.Statistics = new VideoStatistics()
                {
                    FavouriteCount = (int) jsonSubObject.SelectToken("favoriteCount"),
                    ViewCount = (int) jsonSubObject.SelectToken("viewCount")
                };
            }
            if (jsonObject.SelectToken("yt$rating") != null)
            {
                var jsonSubObject = jsonObject.SelectToken("yt$rating");
                deserializedVideo.Rating = new VideoRating()
                {
                    Likes = (int) jsonSubObject.SelectToken("numLikes"),
                    Dislikes = (int) jsonSubObject.SelectToken("numDislikes"),
                };
            }

            return deserializedVideo;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Video).IsAssignableFrom(objectType);
        }
    }
}
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Potato.Dashboard.Models.Twitter
{
    [JsonConverter(typeof(JsonTwitterTweetConverter))]
    public class Tweet : AbstractExtensions
    {
        public static readonly bool noUserDetailsInTweet = true;

        public string Id { get; set; }
        public DateTime Published { get; set; }
        public string Text { get; set; }
        public Uri Link { get; set; }
        public TweetStatistics Statistics { get; set; }
        public TweetEntities Entities { get; set; }
        public User User { get; set; }
        public string HowLongSincePublished { get; set; }

        public void calculateHowLongSincePublished()
        {
            int timeDifference;
            if (DateTime.Now.Year - Published.Year > 0 && (DateTime.Now - Published).TotalDays > 365)
            {
                HowLongSincePublished = "> a year ago";
            }
            else if ((timeDifference = DateTime.Now.Month - Published.Month) > 0)
            {
                HowLongSincePublished = timeDifference == 1 ? "last month" : timeDifference.ToString() + " months ago";
            }
            else if ((timeDifference = DateTime.Now.Day - Published.Day) > 0)
            {
                HowLongSincePublished = timeDifference.ToString() + (timeDifference == 1 ? " day" : " days") + " ago";
            }
            else if ((timeDifference = DateTime.Now.Hour - Published.Hour) > 0)
            {
                HowLongSincePublished = timeDifference.ToString() + (timeDifference == 1 ? " hour" : " hours") + " ago";
            }
            else if ((timeDifference = DateTime.Now.Minute - Published.Minute) > 0)
            {
                HowLongSincePublished = timeDifference.ToString() + (timeDifference == 1 ? " min" : " mins") + " ago";
            }
            else
            {
                HowLongSincePublished = "< 1 min ago";
            }
        }
    }

    public class JsonTwitterTweetConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Serializing to JSON was not needed and has not yet been implemented.");
        }

        // Deserialize - JSON to C# object mapping.
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            System.Diagnostics.Debug.WriteLine(jsonObject);

            // Populate C# object with according JObject data.
            var deserializedTweet = new Tweet()
            {
                Id = (string) jsonObject.SelectToken("id_str"),
                Published = ((string) jsonObject.SelectToken("created_at")).ParseTwitterTime(),
                Text = (string) jsonObject.SelectToken("text"),
                Statistics = new TweetStatistics()
                {
                    RetweetsCount = (int) jsonObject.SelectToken("retweet_count"),
                    FavouritesCount = (int) jsonObject.SelectToken("favorite_count")
                },
                User = new User()
                {
                    Id = (string) jsonObject.SelectToken("user").SelectToken("id_str"),
                    Name = (string) jsonObject.SelectToken("user").SelectToken("name"),
                    ScreenName = "@" + (string) jsonObject.SelectToken("user").SelectToken("screen_name"),
                }
            };

            if (!Tweet.noUserDetailsInTweet)
            {
                // Add a User object associated with the tweet.
                var jsonSubObject = jsonObject.SelectToken("user");
                deserializedTweet.User = new User()
                {
                        Id = (string) jsonSubObject.SelectToken("id_str"),
                        Name = (string) jsonSubObject.SelectToken("name"),
                        ScreenName = "@" + (string) jsonSubObject.SelectToken("screen_name"),
                        Description = (string) jsonSubObject.SelectToken("description"),
                        Joined = ((string) jsonSubObject.SelectToken("created_at")).ParseTwitterTime(),
                        Url = new Uri((string) jsonSubObject.SelectToken("entities").SelectToken("url").SelectToken("urls").First.SelectToken("expanded_url")),
                        Statistics = new UserStatistics()
                        {
                            FollowersCount = (int) jsonSubObject.SelectToken("followers_count"),
                            FriendsCount = (int) jsonSubObject.SelectToken("friends_count"),
                            ListedCount = (int) jsonSubObject.SelectToken("listed_count"),
                            TweetStatusesCount = (int) jsonSubObject.SelectToken("statuses_count")
                        },
                        Profile = new UserProfile()
                        {
                            Image = new Uri(((string) jsonSubObject.SelectToken("profile_image_url")).Replace("_normal", "")),
                            ImageInTweet = new Uri((string) jsonSubObject.SelectToken("profile_image_url")),
                            BackgroundImage = new Uri((string) jsonSubObject.SelectToken("profile_background_image_url")),
                            BackgroundColor = (string) jsonSubObject.SelectToken("profile_background_color"),
                            LinkColor = (string) jsonSubObject.SelectToken("profile_link_color"),
                            TextColor = (string) jsonSubObject.SelectToken("profile_text_color"),
                            SidebarFillColor = (string) jsonSubObject.SelectToken("profile_sidebar_fill_color"),
                            SidebarBorderColor = (string) jsonSubObject.SelectToken("profile_sidebar_border_color")
                        }
                    };
            }

            // Include all types of Tweet Entities.
            deserializedTweet.Entities = new TweetEntities() { };
            if (jsonObject.SelectToken("entities").SelectToken("hashtags").HasValues)
            {
                var jsonSubObject = jsonObject.SelectToken("entities").SelectToken("hashtags");
                foreach (var entry in jsonSubObject)
                {
                    deserializedTweet.Entities.Hashtags.Add(new Hashtag()
                    {
                        Text = (string) entry.First()
                    });
                }
            }
            if (jsonObject.SelectToken("entities").SelectToken("symbols").HasValues)
            {
                var jsonSubObject = jsonObject.SelectToken("entities").SelectToken("symbols");
                foreach (var entry in jsonSubObject)
                {
                    deserializedTweet.Entities.Symbols.Add(new Symbol()
                    {
                        Text = (string) entry.First()
                    });
                }

            }
            if (jsonObject.SelectToken("entities").SelectToken("urls").HasValues)
            {
                var jsonSubObject = jsonObject.SelectToken("entities").SelectToken("urls");
                foreach (var entry in jsonSubObject)
                {
                    deserializedTweet.Entities.Urls.Add(new Url()
                    {
                        TargetUrl = new Uri((string) entry.SelectToken("expanded_url")),
                        DisplayUrl = (string) entry.SelectToken("display_url"),
                        ExtractedUrl = new Uri((string) entry.SelectToken("url"))
                    });
                }
            }
            if (jsonObject.SelectToken("entities").SelectToken("user_mentions").HasValues)
            {
                var jsonSubObject = jsonObject.SelectToken("entities").SelectToken("user_mentions");
                foreach (var entry in jsonSubObject)
                {
                    deserializedTweet.Entities.UserMentions.Add(new UserMention()
                    {
                        Id = (string) entry.SelectToken("id_str"),
                        Name = (string) entry.SelectToken("name"),
                        ScreenName = (string) entry.SelectToken("screen_name"),
                    });
                }
            }
            if (jsonObject.SelectToken("entities").SelectToken("media") != null)
            {
                var jsonSubObject = jsonObject.SelectToken("entities").SelectToken("media");
                foreach (var entry in jsonSubObject)
                {
                    deserializedTweet.Entities.Media.Add(new Media()
                    {
                        Id = (string) entry.SelectToken("id_str"),
                        Source = new Uri((string) entry.SelectToken("media_url")),
                        DisplayUrl = (string) entry.SelectToken("display_url"),
                        ExtractedUrl = new Uri((string) entry.SelectToken("url")),
                    });
                }
            }

            deserializedTweet.User.Link = new Uri("https://www.twitter.com/" + deserializedTweet.User.ScreenName);
            deserializedTweet.Link = new Uri(deserializedTweet.User.Link + "/status/" + deserializedTweet.Id);

            return deserializedTweet;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Tweet).IsAssignableFrom(objectType);
        }
    }
}
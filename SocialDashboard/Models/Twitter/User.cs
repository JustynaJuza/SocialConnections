using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialDashboard.Models.Twitter
{
    [JsonConverter(typeof(JsonTwitterUserConverter))]
    public class User : AbstractExtensions
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ScreenName { get; set; }
        public string Description { get; set; }
        public DateTime? Joined { get; set; }
        public Uri Url { get; set; }
        public Uri Link { get; set; }
        public UserProfile Profile { get; set; }
        public UserStatistics Statistics { get; set; }
    }

    public class JsonTwitterUserConverter : JsonConverter
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
            var deserializedUser = new User()
            {
                Id = (string) jsonObject.SelectToken("id_str"),
                Name = (string) jsonObject.SelectToken("name"),
                ScreenName = "@" + (string) jsonObject.SelectToken("screen_name"),
                Description = (string) jsonObject.SelectToken("description"),
                Joined = ((string) jsonObject.SelectToken("created_at")).ParseTwitterTime(),
                Url = new Uri((string) jsonObject.SelectToken("entities").SelectToken("url").SelectToken("urls").First.SelectToken("expanded_url")),
                Statistics = new UserStatistics()
                {
                    FollowersCount = (int) jsonObject.SelectToken("followers_count"),
                    FriendsCount = (int) jsonObject.SelectToken("friends_count"),
                    ListedCount = (int) jsonObject.SelectToken("listed_count"),
                    TweetStatusesCount = (int) jsonObject.SelectToken("statuses_count")
                },
                Profile = new UserProfile()
                {
                    Image = new Uri(((string) jsonObject.SelectToken("profile_image_url")).Replace("_normal", "")),
                    ImageInTweet = new Uri((string) jsonObject.SelectToken("profile_image_url")),
                    BackgroundImage = new Uri((string) jsonObject.SelectToken("profile_background_image_url")),
                    BackgroundColor = (string) jsonObject.SelectToken("profile_background_color"),
                    LinkColor = (string) jsonObject.SelectToken("profile_link_color"),
                    TextColor = (string) jsonObject.SelectToken("profile_text_color"),
                    SidebarFillColor = (string) jsonObject.SelectToken("profile_sidebar_fill_color"),
                    SidebarBorderColor = (string) jsonObject.SelectToken("profile_sidebar_border_color")
                }
            };
            deserializedUser.Link = new Uri("https://www.twitter.com/" + deserializedUser.ScreenName);

            return deserializedUser;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Tweet).IsAssignableFrom(objectType);
        }
    }
}
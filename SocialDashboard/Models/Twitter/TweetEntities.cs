using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialAlliance.Models.Twitter
{
    public class TweetEntities : AbstractExtensions
    {
        public IList<Hashtag> Hashtags { get; set; }
        public IList<Symbol> Symbols { get; set; }
        public IList<Url> Urls { get; set; }
        public IList<UserMention> UserMentions { get; set; }
        public IList<Media> Media { get; set; }

        // Constructor.
        public TweetEntities()
        {
            Hashtags = new List<Hashtag>();
            Symbols = new List<Symbol>();
            Urls = new List<Url>();
            UserMentions = new List<UserMention>();
            Media = new List<Media>();
        }
        // Constructor with all lists.
        public TweetEntities(IList<Hashtag> hashtags, IList<Symbol> symbols, IList<Url> urls, IList<UserMention> userMentions, IList<Media> media)
        {
            Hashtags = hashtags;
            Symbols = symbols;
            Urls = urls;
            UserMentions = userMentions;
            Media = media;
        }
    }

    public interface ITweetEntity
    {
    }

    public class Hashtag : ITweetEntity
    {
        public string Text { get; set; }
    }

    public class Symbol : ITweetEntity
    {
        public string Text { get; set; }
    }

    public class Url : ITweetEntity
    {
        public Uri TargetUrl { get; set; }
        public Uri ExtractedUrl { get; set; }
        public string DisplayUrl { get; set; }
    }

    public class UserMention : ITweetEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ScreenName { get; set; }
    }

    public class Media : ITweetEntity
    {
        public string Id { get; set; }
        public Uri Source { get; set; }
        public Uri ExtractedUrl { get; set; }
        public string DisplayUrl { get; set; }
    }
}
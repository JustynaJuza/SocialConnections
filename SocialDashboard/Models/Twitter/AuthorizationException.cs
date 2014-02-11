using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace SocialAlliance.Models.Twitter
{
    [Serializable()]
    public class AuthorizationException : Exception
    {
        public AuthorizationException() : base() { }
        public AuthorizationException(string message) : base(message) { }

        protected AuthorizationException(SerializationInfo info, StreamingContext context) { }
    }
}
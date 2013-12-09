using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace SocialDashboard.Models
{
    // Overridong ToString allows to see all properties of an object also in Debug mode, not only when explicitly using the extension method.
    public abstract class AbstractExtensions
    {
        public override string ToString()
        {
            string toString = this.GetType().FullName + ": ";
            var classMembers = this.GetType().GetProperties();

            foreach (System.Reflection.PropertyInfo member in classMembers)
                toString += "\n" + member.Name + " : " + member.GetValue(this) + "; ";

            return toString;
        }
    }

    public static class Extensions
    {
        public static string ToFullString(this object obj)
        {
            string toString = obj.GetType().FullName + ": ";
            var classMembers = obj.GetType().GetProperties();

            foreach (System.Reflection.PropertyInfo member in classMembers)
                toString += "\n" + member.Name + " : " + member.GetValue(obj) + "; ";

            return toString;
        }

        public static DateTime ParseTwitterTime(this string date)
        {
            const string format = "ddd MMM dd HH:mm:ss zzzz yyyy";
            return DateTime.ParseExact(date, format, CultureInfo.InvariantCulture);
        }
    }
}
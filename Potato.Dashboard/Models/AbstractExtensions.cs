using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Potato.Dashboard.Models
{
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

    public static class ObjectExtension
    {
        public static string ToFullString(this object myObj)
        {
            string toString = myObj.GetType().FullName + ": ";
            var classMembers = myObj.GetType().GetProperties();

            foreach (System.Reflection.PropertyInfo member in classMembers)
                toString += "\n" + member.Name + " : " + member.GetValue(myObj) + "; ";

            return toString;
        }
    }
}
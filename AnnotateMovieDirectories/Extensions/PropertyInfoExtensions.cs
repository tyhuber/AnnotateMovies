using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Xml.Serialization;

namespace AnnotateMovieDirectories.Extensions
{
    /*public static class PropertyInfoExtensions
    {
        public static bool IsAnyDefined(this PropertyInfo prop, params Type[] types)
        {
            return types.Any(x => prop.IsDefined(x, true));
        }

        public static IEnumerable<PropertyInfo> GetXmlElements(this Type t)
        {
            return
                t.GetProperties().Where(
                    x =>
                        !x.IsDefined(typeof (XmlIgnoreAttribute)) &&
                        !x.IsDefined(typeof (XmlArrayItemAttribute)));
        }



       /* public static IEnumerable<PropertyInfo> GetXmlElements(this Type t)
        {
            /*return
                t.GetProperties().Where(
                    x => x.IsDefined(typeof (XmlA)) || x.IsDefined(typeof (XmlElement)));#2#
                //x.IsAnyDefined(typeof (XmlAttribute), typeof (XmlElement)));
        }#1#

        public static string GetNameValue(this PropertyInfo prop, object obj)
        {
            string name = prop.Name;
            string value = Convert.ChangeType(prop.GetValue(obj), prop.GetType()).ToString();
            return $"\t-{name}:{value}";
        }
        public static IEnumerable<PropertyInfo> GetIEnumerables(this Type t)
        {
            return t.GetProperties().Where(x => x.PropertyType.GetInterfaces().Contains(typeof(IEnumerable))); //x.IsDefined(typeof (XmlArrayItemAttribute)));
        }
    }*/
}
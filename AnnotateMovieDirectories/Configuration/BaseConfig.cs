using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using AnnotateMovieDirectories.Logging;
using YamlDotNet.Serialization;

namespace AnnotateMovieDirectories.Configuration
{
    public abstract class BaseConfig
    {
        public T Convert<T>() where T:BaseConfig,new()
        {
            var properties =
                GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                         .Where(ConvertableXmlProperty).ToList();
            Console.WriteLine($"For {GetType().Name} - got {properties.Count} writeable properties.");
            var tProperties =
                typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                         .Where(ConvertableYamlProperty).ToList();
            Console.WriteLine($"For {typeof(T).Name} - got {properties.Count} writeable properties.");
            var propertyComparer = new PropertyComparer();
            var intersect = properties.Intersect(tProperties, propertyComparer).ToList();
            Console.WriteLine($"Got {intersect.Count()} intersecting properties");

            var both = intersect.Select(x =>
            {
                var t = tProperties.First(y => propertyComparer.Equals(x, y));
                return new {One = x, Two = t};
            }).ToList();
            T other = new T();
            foreach (var prop in both)
            {
                if (prop.One.PropertyType.BaseType==typeof(BaseConfig))
                {
                    /*Console.WriteLine($"Property {prop.One.Name} in {GetType().Name} base type = BaseConfig");
                    var baseConfig = prop.One.GetValue(this, null) as BaseConfig;
                    if (baseConfig == null)
                    {
                        Console.Error.WriteLine($"Failed to convert {prop.One.Name} to base config.");
                        continue;
                    }
                    var convert = baseConfig.Convert<T>();
                    Console.WriteLine($"Setting {prop.Two.Name}");
                    prop.Two.SetValue(other,convert);*/
                    continue;
                }
                var val = System.Convert.ChangeType(prop.One.GetValue(this, null),prop.One.PropertyType);
                Console.WriteLine($"Property {prop.One.Name} in {GetType().Name} = {val}.");
                Console.WriteLine($"Setting property {prop.Two.Name} in {typeof(T).Name} to {val}");
                prop.Two.SetValue(other,val);
            }
            return other;


        }

        private static bool ConvertableXmlProperty(PropertyInfo property)
        {
            if (!property.CanWrite) return false;
            return property.IsDefined(typeof(XmlElementAttribute))
                   || property.IsDefined(typeof(XmlAttributeAttribute))
                   || property.IsDefined(typeof(XmlArrayAttribute))
                   || property.IsDefined(typeof(XmlArrayItemAttribute));
        }

        private bool ConvertableYamlProperty(PropertyInfo prop)
        {
            if (!prop.CanWrite) return false;
            return prop.IsDefined(typeof(YamlMemberAttribute))
                   || prop.IsDefined(typeof(YamlIgnoreAttribute));
        }


        public override string ToString()
        {
            return string.Join("\n", Reflect());
        }

        protected List<string> Reflect()
        {
            var props = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => !x.IsDefined(typeof(XmlIgnoreAttribute)));
            List<string> nameVals = new List<string>();
            foreach (var p in props)
            {
                if (ReflectIEnumerable(p, ref nameVals)) continue;
                //                BLog($"{p.Name}-{p.GetValue(this,null)}");
                nameVals.Add($"{p.Name}: {p.GetValue(this, null)}");
            }
            return nameVals;
        }

        protected bool ReflectIEnumerable(PropertyInfo p, ref List<string> nameVals)
        {
            if (p.PropertyType == typeof(string) ||
                !p.PropertyType.GetInterfaces().Contains(typeof(IEnumerable))) return false;
            nameVals.Add($"{p.Name}:");
            foreach (var s in (IEnumerable)p.GetValue(this, null))
            {
                nameVals.Add($"  -{s}");
            }
            return true;
        }

        protected static void Log(string s, [CallerMemberName] string name = "", [CallerLineNumber] int ln = 0,
            [CallerFilePath] string path = "")
        {
            Logger.BLog(s, name, path, ln);
        }

        protected static void BLog(string s)
        {
            Logger.BLog(s);
        }

        protected static void Error(string s, [CallerMemberName] string name = "", [CallerLineNumber] int ln = 0,
            [CallerFilePath] string path = "")
        {
            Logger.BError(s, name, path, ln);
        }
    }

    public class PropertyComparer : IEqualityComparer<PropertyInfo>
    {
        public bool Equals(PropertyInfo x, PropertyInfo y)
        {
            return x.Name.ToLowerInvariant().Equals(y.Name.ToLowerInvariant());
        }

        public int GetHashCode(PropertyInfo obj)
        {
            return obj.Name.ToLowerInvariant().GetHashCode();
        }
    }
}
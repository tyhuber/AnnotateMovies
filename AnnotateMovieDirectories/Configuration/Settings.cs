using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace AnnotateMovieDirectories.Configuration
{
    public class Settings : BaseConfig
    {
        [XmlAttribute]
        public string Path { get; set; }
        [XmlAttribute]
        public bool Overwrite { get; set; }
        [XmlAttribute]
        public bool Test { get; set; }
        [XmlElement]
        public MetaCritic MetaCritic { get; set; }
        [XmlElement]
        public Weights Weights { get; set; }
        /*private static void Log(string s, [CallerMemberName] string name = "", [CallerLineNumber] int ln = 0,
           [CallerFilePath] string path = "")
        {
            Logger.BLog(s, name, path, ln);
        }

        private static void Error(string s, [CallerMemberName] string name = "", [CallerLineNumber] int ln = 0,
            [CallerFilePath] string path = "")
        {
            Logger.BError(s, name, path, ln);
        }
        public override string ToString()
        {
            return string.Join("\n", Reflect());
        }

        private List<string> Reflect()
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

        private bool ReflectIEnumerable(PropertyInfo p, ref List<string> nameVals)
        {
            if (p.PropertyType != typeof(string) && p.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)))
            {
                //                BLog($"{p.Name}:");
                nameVals.Add($"{p.Name}:");
                foreach (var s in (IEnumerable)p.GetValue(this, null))
                {
                    //                    BLog($"\t{s}");
                    nameVals.Add($"  -{s}");
                }
                return true;
            }
            return false;
        }*/


    }

    public class MetaCritic:BaseConfig
    {
        [XmlAttribute]
        public bool Add { get; set; }
        [XmlAttribute]

        public bool Use { get; set; }
    }

    public class Weights:BaseConfig
    {
        [XmlAttribute]
        public double Imdb { get; set; }
        [XmlAttribute]
        public double RtFresh { get; set; }
        [XmlAttribute]
        public double RtRating { get; set; }
        [XmlAttribute]
        public double MetaCritic { get; set; }

        [XmlIgnore]
        public bool Valid => Math.Abs(Imdb + RtFresh + RtRating + MetaCritic - 1) < double.Epsilon;

    }
}
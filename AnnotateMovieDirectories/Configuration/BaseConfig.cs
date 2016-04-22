using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using AnnotateMovieDirectories.Logging;

namespace AnnotateMovieDirectories.Configuration
{
    public abstract class BaseConfig
    {
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
}
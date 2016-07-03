using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using AnnotateMovieDirectories.Extensions.DirInfo;
using AnnotateMovieDirectories.Logging;

namespace AnnotateMovieDirectories.Movies
{
    public static class OscarTagger
    {
        private static Regex OscarRegex => DirRegex.OscarRegex;

        
        public static void AddOscarNotation(DirectoryInfo dir)
        {
            Log($"Adding oscar tag for {dir.Name} if applicable.");
            if(OscarRegex.IsMatch(dir.Name))return;
            string awards;
            if (!dir.GetOscarsLine(out awards))
            {
                Log($"{dir.Name} did not win oscars.");
                return;
            }
            string tag = GetOscarTag(awards);

            Log($"Appending {tag} to {dir.Name}");
            dir.AppendToName(tag);
        }

        public static string GetOscarTag(string awards)
        {
            Regex winRegex = new Regex(@"Won (?<v>\d{1,2}) Oscars?");
            Regex nomRegex = new Regex(@"Nominated for (?<v>\d{1,2}) Oscars?");
            string tag = string.Empty;
            if (!GenTag(winRegex, awards, ref tag, "wins", "Oscars]"))
            {
                Log($"Attempting to get match for oscar nominations.");
                GenTag(nomRegex, awards, ref tag, "nominations", "Oscar Noms]");
            }
            return tag;
        }

        private static bool GenTag(Regex regex, string awards, ref string tag, string loggingString, string append)
        {
            if (regex.IsMatch(awards))
            {
                Log($"Got match for {loggingString}");
                var match = regex.Match(awards);
                string num = match.Groups["v"].Value;
                tag = $"[{num} {append}";
                return true;
            }
            Log($"No match for {loggingString}");
            return false;
        }

        private static void Log(string s, [CallerMemberName] string name = "",
            [CallerFilePath] string path = "", [CallerLineNumber] int ln = 0)
        {
            Logger.BLog(s, name, path, ln);
        }

        private static void Error(string s, [CallerMemberName] string name = "",
            [CallerFilePath] string path = "", [CallerLineNumber] int ln = 0)
        {
            Logger.BError(s, name, path, ln);
        }

        private static void Error(Exception ex, [CallerMemberName] string name = "",
            [CallerFilePath] string path = "", [CallerLineNumber] int ln = 0)
        {
            Logger.BError(ex, name, path, ln);
        }
    }
}
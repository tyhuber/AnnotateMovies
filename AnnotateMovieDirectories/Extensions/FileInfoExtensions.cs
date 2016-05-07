using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using AnnotateMovieDirectories.Extensions.Quality;
using AnnotateMovieDirectories.Logging;

namespace AnnotateMovieDirectories.Extensions
{
    public static class FileInfoExtensions
    {
        private static IEnumerable<string> VideoExtensions => new List<string> { "mkv", "avi", "mp4", "mpg", "mov", "wmv" };
        private static IEnumerable<string> AudioExtensions => new List<string> {"aac","mp3","wma","flac","wav","aa","3gp","m4a", "m4b", "mpc","ogg","oga"};
        private static Regex RipRegex => new Regex(@"(BluRay|BRRip|BDrip|DVDRip|DVDSCR|WEBRip|HDRip)", RegexOptions.IgnoreCase);
        private static Regex QRegex => new Regex(@"(720|480|1080|1\d{3})p");
        private static Regex YearRegex => new Regex(@"(19|20)\d{2}");
        public static bool IsVideo(this FileInfo file, bool log = true)
        {
            Regex sampleRegex = new Regex("sampe",RegexOptions.IgnoreCase);
            if (file.Name.EndsWith(".mobi") || file.Name.EndsWith(".epub") || file.Name.EndsWith(".pdf")||sampleRegex.IsMatch(file.Name))
            {
                return false;
            }
            try
            {
                string ext = Path.GetExtension(file.FullName);
                if (log)
                {
                    Log($"Extension for {file.String()} is {ext}");
                }
                if (VideoExtensions.Any(x => ext.Contains(x)))
                {
                    if (log)
                    {
                        Log($"{file.String()} contains a video extension. Ext = {ext}");
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Error(e);
                return false;
            }
            /*return (ext.Contains("mkv") || ext.Contains("avi") || ext.Contains("mp4") || ext.Contains("mpg") ||
                                   ext.Contains("mov") || ext.Contains("wmv"));*/
        }

        public static bool IsAudio(this FileInfo file)
        {
            return AudioExtensions.Any(x => x.Equals(file.Extension, StringComparison.OrdinalIgnoreCase));
        }

        public static string GetNameWithoutExt(this FileInfo file)
        {
            return file.Name.Replace(file.Extension, string.Empty);
        }


        public static bool TryGetExt(this FileInfo file, out string ext)
        {
            ext = VideoExtensions.FirstOrDefault(file.Name.Contains);
            return string.IsNullOrWhiteSpace(ext);
        }

        public static void RenameVideo(this FileInfo file)
        {
            Log($"Renaming/moving video file {file.String()}");
            string year = file.MatchRegex(YearRegex);
            Log($"Year = {year}");
            string quality = file.MatchRegex(QRegex);
            Log($"Quality = {quality}");
            string ripType = file.MatchRegex(RipRegex);
            RipType type = Rip.ParseType(ripType);
            Log($"RipType = {type}");
            string ripTypeString = type == RipType.Unknown ? string.Empty : type.ToString();
            Log($"RipTypeString = {ripTypeString}");
            string ext;
            if (!file.TryGetExt(out ext))
            {
                Error($"Unable to find correct video extension for {file.String()}");
                return;
            }
            string noExt = file.GetNameWithoutExt();
            string name = noExt;
            TryReplace(year, ref name);
            TryReplace(quality, ref name);
            TryReplace(ripTypeString, ref name);
            name = name.Replace('.', ' ');
            Log($"Spaces instead of . = {name}");

            string videoName = $"{name} {SurroundWith('[', ']', quality, ripTypeString)} {SurroundWith('(', ')', year)}";
            Log($"New video name = {videoName}");
           
        }

        private static void TryReplace(string ripTypeString, ref string name)
        {
            if (!string.IsNullOrWhiteSpace(ripTypeString))
            {
                name = name.Replace(ripTypeString, string.Empty);
                Log($"Replaced {ripTypeString}. Replaced name = {name}");
            }
        }

        private static void Log(string s, [CallerMemberName]
        string name = "",
            [CallerFilePath] string path = "", [CallerLineNumber] int ln = 0)
        {
            Logger.BLog(s, name, path, ln);
        }
        private static void Error(string s, [CallerMemberName]
        string name = "",
            [CallerFilePath] string path = "", [CallerLineNumber] int ln = 0)
        {
            Logger.BError(s, name, path, ln);
        }
        private static void Error(Exception ex, [CallerMemberName]
        string name = "",
            [CallerFilePath] string path = "", [CallerLineNumber] int ln = 0)
        {
            Logger.BError(ex, name, path, ln);
        }

        
        


       
        public static string ShortName(this FileInfo file)
        {
            int l = 89;
            Regex beginTrim = new Regex(@"^\s*\[\s*www\.\S+\s\]");
            Regex ext = new Regex(@"\.(mkv|avi|mp4|mpg|mov|wmv)");
            Regex ripRegex = new Regex(@"\[(\d+p\s)?(BluRay|BRRip|BDrip|DVDRip|DVDSCR|WEBRip|HDRip)\]", RegexOptions.IgnoreCase);
            Regex time = new Regex(@"\(\d{2}-\d{2}-\d{2}");
            Regex qRegex = new Regex(@"(720|480|1080)p");
            Regex xReg = new Regex(@"(\.|\s|-)x\w{3}(\.|\s|-)");
            Regex spaceReg = new Regex(@"\s{2,}");
            Regex braceReg = new Regex(@"((\(|\[|\{)\S+(\)|\]|\}))?");
            string name = file.Name;
            if (beginTrim.IsMatch(name)) name = beginTrim.Replace(name, string.Empty);
            if (ext.IsMatch(name)) name = ext.Replace(name, string.Empty);
            if (ripRegex.IsMatch(name)) name = ripRegex.Replace(name, string.Empty);
            if (time.IsMatch(name)) name = time.Replace(name, string.Empty);
            if (qRegex.IsMatch(name)) name = qRegex.Replace(name, string.Empty);
            if (xReg.IsMatch(name)) name = xReg.Replace(name, string.Empty);
            if (spaceReg.IsMatch(name)) name = spaceReg.Replace(name, " ");
            if (braceReg.IsMatch(name)) name = braceReg.Replace(name, string.Empty);



            if (name.Length > l)
            {
                return name.Substring(0, l);
            }
            return name.PadRight(l);
        }
        public static string String(this FileInfo file) => file.FullName;

        private static string SurroundWith(char open, char close, params string[] values)
        {
            if (!values.Any()) return string.Empty;
            if (values.All(string.IsNullOrWhiteSpace)) return string.Empty;
            return $"{open}{string.Join(" ", values.Where(x => !string.IsNullOrWhiteSpace(x)))}{close}";
        }

        private static string MatchRegex(this FileInfo file, Regex reg)
        {
            return reg.IsMatch(file.Name) ? reg.Match(file.Name).Value : string.Empty;
        }
    }

    
}
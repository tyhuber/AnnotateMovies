using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace AnnotateMovieDirectories.Extensions.DirInfo
{
    public static class DirRegex
    {
        private static IEnumerable<string> VideoExtensions => new List<string> { "mkv", "avi", "mp4", "mpg", "mov", "wmv" };
        public static Regex RipRegex => new Regex(@"(BluRay|BRRip|BDrip|DVDRip|DVDSCR|WEBRip|HDRip|x264)", RegexOptions.IgnoreCase);
        public static Regex QRegex => new Regex(@"(?<v>(720|480|1080|1\d{3})p)");
        public static Regex YearRegex => new Regex(@"\(?(?<v>(19|20)\d{2})\)?");
        public static Regex TimeRegex => new Regex(@"\[(?<v>\d{2,3} min)\]");
        public static Regex ForeignRegex => new Regex(@"[\(|\[](?!(19|20)\d{2})((\w+[\s|\.]?)+)[\)|\]]");
        public static Regex TvRegex => new Regex(@"(S0\d|Season)(E\d{1,2})?");
        public static Regex ScoreRegex => new Regex(@"^(?<v>[0-9]{2}(\.[0-9],)?).*");
        private static Regex AnnotationRegex => new Regex(@"(?<title>.*)\((?<year>(19|20)\d{2})\)\s+\[(?<time>\d+) min\]\sIMDB-(?<imdb>[1-9]\.\d)\s?(?<rt>RT-(?<rtfresh>\d{2})%\s\((?<rtrating>\d\.\d)\))?");

        public static bool IsAnnotated(this DirectoryInfo dir)
        {
            return AnnotationRegex.IsMatch(dir.Name);
        }

        public static bool MatchRegex(this DirectoryInfo dir, Regex reg, out string match)
        {
            match = string.Empty;
            if (!reg.IsMatch(dir.Name)) return false;
            match = reg.Match(dir.Name).Value;
            return true;
        }

        public static int MatchRegex(this DirectoryInfo dir, Regex reg)
        {
            if (!reg.IsMatch(dir.Name)) return -1;
            return reg.Match(dir.Name).Index;
        }

        public enum RegexType
        {
            Rip,
            Quality,
            Year,
            Time,
            Foreign,
            NoMatch
        }

        public static Regex GetRegex(this RegexType t)
        {
            switch (t)
            {
                case RegexType.Rip:
                    return DirRegex.RipRegex;
                case RegexType.Quality:
                    return DirRegex.QRegex;
                case RegexType.Year:
                    return DirRegex.YearRegex;
                case RegexType.Time:
                    return DirRegex.TimeRegex;
                case RegexType.Foreign:
                    return DirRegex.ForeignRegex;
                default:
                    throw new ArgumentOutOfRangeException(nameof(t), t, null);
            }
        }

        public static int GetIndex(this RegexType t, DirectoryInfo dir)
        {
            return dir.MatchRegex(t.GetRegex());
        }
    }
}
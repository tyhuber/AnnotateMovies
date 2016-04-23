using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using AnnotateMovieDirectories.Configuration;
using AnnotateMovieDirectories.Logging;

namespace AnnotateMovieDirectories.Extensions.DirInfo
{
    public static class DirMovie
    {
        public static Regex GetFirstRegexMatch(this DirectoryInfo dir)
        {
            Regex firstRegex = default(Regex);
            int firstIndex = -1;
            DirRegex.RegexType type = DirRegex.RegexType.NoMatch;

            foreach (DirRegex.RegexType reg in Enum.GetValues(typeof(DirRegex.RegexType)))
            {
                if (reg.Equals(DirRegex.RegexType.NoMatch)) continue;
                int index = reg.GetIndex(dir);
                if (firstIndex == -1 && index > 0)
                {
                    firstIndex = index;
                    firstRegex = reg.GetRegex();
                    type = reg;
                }
                else if (index > 0 && index < firstIndex)
                {
                    firstIndex = index;
                    firstRegex = reg.GetRegex();
                    type = reg;
                }
            }
            Log($"First regex match is {type} at index {firstIndex}");
            return firstRegex;
        }

        public static void SortByName(this DirectoryInfo dir)
        {
            var movie = dir.GetMovie();
            if (!movie.Annotated) return;
            switch (Cfg.Config.RenameBy)
            {
                case RenameBy.Score:
                    dir.MoveTo(Path.Combine(dir.Parent.FullName, movie.ScoreTitle));
                    break;
                case RenameBy.Runtime:
                    dir.MoveTo(Path.Combine(dir.Parent.FullName, movie.RunTimeTitle));
                    break;
                case RenameBy.Year:
                    dir.MoveTo(Path.Combine(dir.Parent.FullName, movie.YearTitle));
                    break;
                case RenameBy.Normal:
                    dir.MoveTo(Path.Combine(dir.Parent.FullName, movie.NormalTitle));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Cfg.Config.RenameBy), Cfg.Config.RenameBy, null);
            }
        }

        public static bool TryGetYear(this DirectoryInfo dir, out string year)
        {
            string tmp;
            year = String.Empty;
            if (dir.MatchRegex(DirRegex.YearRegex, out tmp))
            {
                year = tmp.Trim('(', ')');
                return true;
            }
            return false;
        }
        public static bool GetMovieName(this DirectoryInfo dir, out string name)
        {
            Log($"Attempting to get movie name for {dir.Name}");
            name = String.Empty;
            if (DirRegex.ScoreRegex.IsMatch(dir.Name)) name= DirRegex.ScoreRegex.Replace(name, String.Empty);
            Regex first = dir.GetFirstRegexMatch();
            if (first == default(Regex))
            {
                return false;
            }
            var split = first.Split(dir.Name);
            name = split[0].Replace('.', ' ');
            Log($"Got movie name {name} for {dir.Name}");
            return true;
        }

        public static bool GetNameYearAnnotated(this DirectoryInfo dir, out string name, out string year)
        {
            name = string.Empty;
            year = string.Empty;
            if (!dir.HasScore()) return false;
            string score = DirRegex.ScoreRegex.Match(dir.Name).Groups["v"].Value;
            name = dir.Name.Replace(score, string.Empty).Trim();
            name = DirRegex.YearRegex.Split(name)[0];
            year = DirRegex.YearRegex.Match(dir.Name).Groups["v"].Value;
            return !name.IsNull() && !year.IsNull();
        }

        public static bool HasScore(this DirectoryInfo dir)
        {
            return DirRegex.ScoreRegex.IsMatch(dir.Name);
        }

        public static void RenameByScore(this DirectoryInfo dir)
        {
            var movie = dir.GetMovie();
            dir.MoveTo(movie.ScoreTitle);
        }

        public static bool IsTv(this DirectoryInfo dir)
        {
            return DirRegex.TvRegex.IsMatch(dir.Name);
        }

        public static FileInfo GetVideo(this DirectoryInfo dir)
        {
            return dir.EnumerateFiles().FirstOrDefault(x => x.IsVideo());
        }

        public static bool ContainsMovie(this DirectoryInfo dir)
        {
            return dir.EnumerateFiles().Any(x => x.IsVideo());
        }

        public static IEnumerable<DirectoryInfo> EnumerateMovieDirectories(this DirectoryInfo dir)
        {
            return dir.EnumerateDirectories().Where(x => !x.Skip());
        }

        public static IEnumerable<DirectoryInfo> EnumerateVideoDirectories(this DirectoryInfo dir)
        {
            return dir.EnumerateDirectories().Where(x => x.ContainsMovie() && !x.Skip());
        }

        public static IEnumerable<DirectoryInfo> EnumerateAnnotatedDirectories(this DirectoryInfo dir)
        {
            return dir.EnumerateDirectories().Where(x => DirRegex.ScoreRegex.IsMatch(x.Name));
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
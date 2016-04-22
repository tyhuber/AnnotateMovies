using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using AnnotateMovieDirectories.Attributes;
using AnnotateMovieDirectories.Configuration;
using AnnotateMovieDirectories.Logging;
using AnnotateMovieDirectories.Omdb;
using AnnotateMovieDirectories.Omdb.Metacritic;
using AnnotateMovieDirectories.Tools;

namespace AnnotateMovieDirectories.Extensions
{
    public static class DirectoryInfoExtensions
    {
        private static IEnumerable<string> VideoExtensions => new List<string> { "mkv", "avi", "mp4", "mpg", "mov", "wmv" };
        private static Regex RipRegex => new Regex(@"(BluRay|BRRip|BDrip|DVDRip|DVDSCR|WEBRip|HDRip|x264)", RegexOptions.IgnoreCase);
        private static Regex QRegex => new Regex(@"(?<v>(720|480|1080|1\d{3})p)");
        private static Regex YearRegex => new Regex(@"\(?(?<v>(19|20)\d{2})\)?");
        private static Regex TimeRegex => new Regex(@"\[(?<v>\d{2,3} min)\]");
        private static Regex ForeignRegex => new Regex(@"[\(|\[](?!(19|20)\d{2})((\w+[\s|\.]?)+)[\)|\]]");

        private static Regex TvRegex => new Regex(@"(S0\d|Season)(E\d{1,2})?");

        private static Regex ScoreRegex => new Regex(@"^(?<v>[0-9]{2}\.[0-9],).*");

        private static bool GetMovieName(this DirectoryInfo dir, out string name)
        {
            Log($"Attempting to get movie name for {dir.Name}");
            name = string.Empty;
            if (ScoreRegex.IsMatch(dir.Name)) name=ScoreRegex.Replace(name, string.Empty);
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
        

        private static bool IsTv(this DirectoryInfo dir)
        {
            return TvRegex.IsMatch(dir.Name);
        }

        public static void Rename(this DirectoryInfo dir)
        {
            if(dir.Checked())return;
            if (dir.IsTv()) return;
            OmdbResult result;
            if (!GetOmdbResult(dir, out result)) return;
//            var movie = new Movie(result);
            string newFolderName = result.GetDirectoryName();
                /*$"{result.Score}, {result.Title} ({result.Year}) [{result.Runtime}] IMDB{result.imdbRating} RT-{result.tomatoMeter}% ({result.tomatoRating}) Meta-{result.Metascore}";*/
            Log($"For {dir.Name}, new name = {newFolderName}");
            try
            {
                string newPath = Path.Combine(dir.Parent.FullName, newFolderName);
                dir.MoveTo(newPath);
                result.WritePlotToFile(new DirectoryInfo(newPath));
            }
            catch (Exception e)
            {
                Error(e);
            }
        }

        private static Movie OldRename(DirectoryInfo dir, OmdbResult result)
        {
            string newFolderName =
                $"{result.Title} ({result.Year}) [{result.Runtime}] IMDB-{result.imdbRating} RT-{result.tomatoMeter}% ({result.tomatoRating})";
            Log($"For {dir.Name}, new name = {newFolderName}");
            try
            {
                string newPath = Path.Combine(dir.Parent.FullName, newFolderName);
                var newDir = new DirectoryInfo(newPath);
                var movie = newDir.GetMovie();
                string metaStr = Math.Abs(movie.MetaCritic) < Double.Epsilon
                    ? string.Empty
                    : $" Meta-{movie.MetaCritic}";
                string finalName = $"{movie.Score}, {newFolderName}{metaStr}";
                string finalPath = Path.Combine(dir.Parent.FullName, finalName);
                dir.MoveTo(finalPath);
                result.WritePlotToFile(new DirectoryInfo(finalPath));
                return movie;
            }
            catch (Exception e)
            {
                Error(e);
                return null;
            }
        }

        public static IEnumerable<DirectoryInfo> EnumerateMovieDirectories(this DirectoryInfo dir)
        {
            return dir.EnumerateDirectories().Where(x => !x.Skip());
        }

        public static IEnumerable<DirectoryInfo> EnumerateVideoDirectories(this DirectoryInfo dir)
        {
            return dir.EnumerateDirectories().Where(x => x.ContainsMovie() && !x.Skip());
        }

        public static void CreateEmptyDir(this DirectoryInfo dir, DirectoryInfo newParent)
        {
            var fi = dir.GetVideo();
            if(fi==default(FileInfo))return;
            newParent.CreateSubdirectory(fi.GetNameWithoutExt());
//            string newPath = Path.Combine(newParentPath, fi.GetNameWithoutExt());
//            var di = new DirectoryInfo(newPath);
//            if(di.Exists)di.Delete(true);
//            di.Create();
        }

        public static FileInfo GetVideo(this DirectoryInfo dir)
        {
            return dir.EnumerateFiles().FirstOrDefault(x => x.IsVideo());
        }

        public static bool ContainsMovie(this DirectoryInfo dir)
        {
            return dir.EnumerateFiles().Any(x => x.IsVideo());
        }

        public static bool Skip(this DirectoryInfo dir)
        {
            return Cfg.Config.IgnoreDirectory(dir)||dir.Checked();
        }

        public static bool Checked(this DirectoryInfo dir)
        {
            return dir.Name.Contains("IMDB")&&ScoreRegex.IsMatch(dir.Name);
        }
        public static void WriteInfoToFile(this DirectoryInfo dir)
        {
            OmdbResult result;
            if (dir.IsTv()) return;
            if (!Cfg.Config.Overwrite)
            {
                string infoFile = Path.Combine(dir.FullName, "MovieInfo.txt");
                if (File.Exists(infoFile))
                {
                    Log($"{infoFile} already exists. Will not overwrite."); 
                }
            }
            if (!GetOmdbResult(dir, out result)) return;
            result.WritePlotToFile(dir);
        }

        private static bool GetOmdbResult(DirectoryInfo dir, out OmdbResult result)
        {
            result = default(OmdbResult);
            if (!dir.Search(out result)) return false;
            return true;
        }

        public static bool Search(this DirectoryInfo dir, out OmdbResult result)
        {
            result = default(OmdbResult);
            string name;
            string year;
            if (!GetMovieName(dir, out name))
            {
                Error($"Could not get movie name for {dir.Name}");
                return false;
            }
            bool gotYear = TryGetYear(dir, out year);
            result = gotYear ? OmdbSearch.Query(name, year) : OmdbSearch.Query(name);
            if (result == default(OmdbResult)||Math.Abs(result.Score) < double.Epsilon)
            {
                Error($"Unable to get omdb result for {dir.Name}");
                return false;
            }
            Log($"For dir {dir.Name} OMDB result = {result} ");
            return true;

        }

        public static bool Meta(this DirectoryInfo dir, out double rating)
        {
            rating = 0;
            string name;
            if (!GetMovieName(dir, out name))
            {
                Error($"Could not get movie name for {dir.Name}");
                return false;
            }
            string year;
            if (!TryGetYear(dir, out year))
            {
                Error($"Could not get movie year for {dir.Name}");
                return false;
            }
            double tmp;
            if (!name.TryGetMeta(year, out tmp)) return false;
            rating = tmp;
            return rating.IsZero();
        }

        

        private static Regex GetFirstRegexMatch(this DirectoryInfo dir)
        {
            Regex firstRegex = default(Regex);
            int firstIndex = -1;
            RegexType type = RegexType.NoMatch;

            foreach (RegexType reg in Enum.GetValues(typeof(RegexType)))
            {
                if(reg.Equals(RegexType.NoMatch))continue;
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

        public static bool TryGetYear(this DirectoryInfo dir, out string year)
        {
            string tmp;
            year = string.Empty;
            if (MatchRegex(dir, YearRegex, out tmp))
            {
                year = tmp.Trim('(',')');
                return true;
            }
            return false;
        }
        private static Regex AnnotationRegex => new Regex(@"(?<title>.*)\((?<year>(19|20)\d{2})\)\s+\[(?<time>\d+) min\]\sIMDB-(?<imdb>[1-9]\.\d)\s?(?<rt>RT-(?<rtfresh>\d{2})%\s\((?<rtrating>\d\.\d)\))?");
        public static bool IsAnnotated(this DirectoryInfo dir)
        {
            return AnnotationRegex.IsMatch(dir.Name);
        }

        public static Movie GetMovie(this DirectoryInfo dir)
        {
            return new Movie(dir);
        }

        public static IEnumerable<Movie> EnumerateMovies(this DirectoryInfo dir)
        {
            return dir.EnumerateDirectories("*IMDB*").Select(x => new Movie(x));
        }

        public static void RenameByScore(this DirectoryInfo dir)
        {
            var movie = dir.GetMovie();
            dir.MoveTo(movie.ScoreTitle);
        }

        public static void SortByName(this DirectoryInfo dir)
        {
            var movie = dir.GetMovie();
            if(!movie.Annotated)return;
            switch (Cfg.Config.RenameBy)
            {
                case RenameBy.Score:
                    dir.MoveTo(Path.Combine(dir.Parent.FullName,movie.ScoreTitle));
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

        public static string String(this DirectoryInfo dir)
        {
            return dir.Name;
        }

        private static bool MatchRegex(this DirectoryInfo dir, Regex reg, out string match)
        {
            match = string.Empty;
            if (!reg.IsMatch(dir.Name)) return false;
            match = reg.Match(dir.Name).Value;
            return true;
        }

        private static int MatchRegex(this DirectoryInfo dir, Regex reg)
        {
            if (!reg.IsMatch(dir.Name)) return -1;
            return reg.Match(dir.Name).Index;
        }


        private static void Log(string s, [CallerMemberName] string name = "", [CallerFilePath] string path = "", [CallerLineNumber] int ln = 0)
        {
            Logger.BLog(s, name, path, ln);
        }

        private static void Error(string s, [CallerMemberName] string name = "", [CallerFilePath] string path = "", [CallerLineNumber] int ln = 0)
        {
            Logger.BError(s, name, path, ln);
        }

        private static void Error(Exception ex, [CallerMemberName] string name = "", [CallerFilePath] string path = "", [CallerLineNumber] int ln = 0)
        {
            Logger.BError(ex, name, path, ln);
        }

        internal enum RegexType
        {
            Rip,
            Quality,
            Year,
            Time,
            Foreign,
            NoMatch
        }

        private static Regex GetRegex(this RegexType t)
        {
            switch (t)
            {
                case RegexType.Rip:
                    return RipRegex;
                case RegexType.Quality:
                    return QRegex;
                case RegexType.Year:
                    return YearRegex;
                case RegexType.Time:
                    return TimeRegex;
                case RegexType.Foreign:
                    return ForeignRegex;
                default:
                    throw new ArgumentOutOfRangeException(nameof(t), t, null);
            }
        }

        private static int GetIndex(this RegexType t, DirectoryInfo dir)
        {
            return dir.MatchRegex(t.GetRegex());
        }
    }
}
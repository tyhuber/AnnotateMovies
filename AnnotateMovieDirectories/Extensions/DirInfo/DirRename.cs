using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using AnnotateMovieDirectories.Configuration;
using AnnotateMovieDirectories.Logging;
using AnnotateMovieDirectories.MovieRatings;
using AnnotateMovieDirectories.Movies.Omb;
using AnnotateMovieDirectories.Omdb;

namespace AnnotateMovieDirectories.Extensions.DirInfo
{
    public static class DirRename
    {
        public static void Rename(this DirectoryInfo dir)
        {
            if(dir.Checked())return;
            OmdbResult result;
            string dirName = dir.Name;
            if (DirRegex.ScoreRegex.IsMatch(dir.Name))
            {
                if(!Cfg.Config.Settings.Rename)return;
                string score = DirRegex.ScoreRegex.Match(dirName).Groups["v"].Value;
                dirName=dirName.Replace(score, string.Empty).Trim();
            }
            if (dir.Name.Contains("IMDB"))
            {
                string name = DirRegex.YearRegex.Split(dirName)[0].Trim();
                string year = DirRegex.YearRegex.Match(dirName).Groups["v"].Value;
                result = OmdbSearch.Query(name, year);
            }
            else
            {
                if (!GetOmdbResult(dir, out result)) return;
            }
            string newFolderName = result.GetDirectoryName();
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

        private static DirectoryInfo GetOldDirName(this DirectoryInfo dir)
        {
            var vid = dir.GetVideo();
            string newPath = Path.Combine(dir.Parent.FullName, vid.GetNameWithoutExt());
            dir.MoveTo(newPath);
            return new DirectoryInfo(newPath);
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
            if (!dir.GetMovieName(out name))
            {
                Error($"Could not get movie name for {dir.Name}");
                return false;
            }
            bool gotYear = dir.TryGetYear(out year);
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
            if (!dir.GetMovieName(out name))
            {
                Error($"Could not get movie name for {dir.Name}");
                return false;
            }
            string year;
            if (!dir.TryGetYear(out year))
            {
                Error($"Could not get movie year for {dir.Name}");
                return false;
            }
            double tmp;
            if (!name.TryGetMeta(year, out tmp)) return false;
            rating = tmp;
            return rating.IsZero();
        }

        public static Movie GetMovie(this DirectoryInfo dir)
        {
            return new Movie(dir);
        }

        public static IEnumerable<Movie> EnumerateMovies(this DirectoryInfo dir)
        {
            return dir.EnumerateDirectories("*IMDB*").Where(x=>!DirRegex.ScoreRegex.IsMatch(x.Name)).Select(x => new Movie(x));
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
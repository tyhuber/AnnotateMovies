using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using AnnotateMovieDirectories.Configuration;
using AnnotateMovieDirectories.Extensions.DirInfo;
using AnnotateMovieDirectories.Logging;

namespace AnnotateMovieDirectories.Movies
{
    public static class GenreMover
    {
        private static Dictionary<DirectoryInfo,List<string>> MoviesDict;
        public static Dictionary<string,DirectoryInfo> GenreToDirDict { get; set; }
        private static bool On => Settings.Config.GenreMoveOn;
        private static bool Force => Settings.Config.GenreMoveForce;
        private static List<Movie> Movies { get; set; } 
        


        public static void Move()
        {
            GenreToDirDict=new Dictionary<string, DirectoryInfo>();
            if (On)
            {
                MoviesDict = Settings.DownloadDir.EnumerateMovieInfoDirectories().ToDictionary(x=>x, GetGenresList);
                Log($"Got {MoviesDict.Count} movies.");
                MoveAllMovies();
            }
        }

        private static void MoveAllMovies()
        {
            foreach (var kv in MoviesDict)
            {
                var dir = kv.Key;
                var genres = kv.Value;
                DirectoryInfo newDir;
                if (Settings.Config.MoveGenres.Contains(genres, out newDir))
                {
                    MoveMovie(newDir, dir);
                }
            }
        }

        private static void MoveMovie(DirectoryInfo newDir, DirectoryInfo dir)
        {
            string newPath = Path.Combine(newDir.FullName, dir.Name);
            string msg = $"{dir.Name} to {newPath}.";
            Log($"Moving {msg}");
            try
            {
                dir.MoveTo(newPath);
            }
            catch (Exception e)
            {
                Error($"Unable to move {msg}. Caught exception - {e}");
            }
        }

        private static List<string> GetGenresList(DirectoryInfo x)
        {
            List<string> genres;
            x.GetGenres(out genres);
            return genres;
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
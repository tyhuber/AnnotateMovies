using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using AnnotateMovieDirectories.Extensions.DirInfo;
using AnnotateMovieDirectories.Logging;

namespace AnnotateMovieDirectories.Movies
{
    public class GenreTagger
    {
        private static Regex GenreRegex => new Regex(@"\[(?!\d+\smin)\w+\]");

        public static void AddGenreToDirectory(DirectoryInfo dir)
        {
            if(GenreRegex.IsMatch(dir.Name))return;
            List<string> genres = new List<string>();
            if (!dir.GetGenres(out genres))
            {
                Error($"No genres found for {dir.Name}");
                return;
            }
            Log($"Got {genres.Count} genres for {dir.Name}.");
            string genreTags = "["+string.Join("] [", genres)+"]";
            Log($"Appending {genreTags} to {dir.Name}");
            dir.AppendToName(genreTags);
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
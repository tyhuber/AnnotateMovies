using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using AnnotateMovieDirectories.Configuration;
using AnnotateMovieDirectories.Extensions;
using AnnotateMovieDirectories.Extensions.DirInfo;
using AnnotateMovieDirectories.Logging;

namespace AnnotateMovieDirectories.Movies
{
    public class Annotater
    {
        public static DirectoryInfo MainDir => new DirectoryInfo(Cfg.Config.Path);

        public static void GetRatingsAndRename()
        {
            Log($"Config settings:\n{Cfg.Config}");
            if (Cfg.Config.Rename)
            {
                /*var dirs = MainDir.GetDirectories("*IMDB*");
                foreach (var dir in dirs)
                {
                    dir.SortByName();
                }*/
            }
            else
            {
                RenameAndWriteInfo();
              /*  if (Cfg.Config.Settings.MetaCritic.Add)
                {
                    var movies = new DirectoryInfo(Cfg.Config.Path).EnumerateMovies();
                    foreach (var m in movies)
                    {
                        Log($"{m.Title} - Score = {m.Score}. Meta = {m.MetaCritic}");
                    }
                }*/
            }      
        }

        private static void RenameAndWriteInfo()
        {

            CreateDirectoriesForVideoFiles();
            var dirs = MainDir.EnumerateMovieDirectories();
            if (!dirs.Any())
            {
                Log($"All subdirectories in {MainDir.FullName} have been already annotated or are configured to be ignored.");
                return;
            }
            foreach (var d in dirs)
            {
                d.Rename();
            }
        }

        private static void CreateDirectoriesForVideoFiles()
        {
            var files = MainDir.GetFiles().Where(x => x.IsVideo());
            if (files.Any())
            {
                foreach (var f in files)
                {
                    try
                    {
                        string newPath = Path.Combine(MainDir.CreateSubdirectory(f.GetNameWithoutExt()).FullName, f.Name);
                        f.MoveTo(newPath);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                    }
                }
            }
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AnnotateMovieDirectories.Configuration;
using AnnotateMovieDirectories.Extensions;
using AnnotateMovieDirectories.Logging;
using AnnotateMovieDirectories.Omdb.Metacritic;

namespace AnnotateMovieDirectories
{
    class Program
    {
        public const string ConfigPath = "Config.xml";
        private const string DownloadPath = @"C:\Users\Ty\Documents\Downloads\To Watch";
        private static readonly DirectoryInfo DownloadDir = new DirectoryInfo(DownloadPath);
        private static DirectoryInfo MainDir => new DirectoryInfo(Cfg.Config.Path);

        static void Main(string[] args)
        {
            Logger.Init("Log.txt");
            Log($"Beginning annotation script");
            if (!File.Exists(ConfigPath))
            {
                Log($"Config does not exist. Creating default config.");
                Cfg.SetDefault();
            }
            else
            {
                Cfg.Deserialize(ConfigPath);
            }
            if (Cfg.Config.Settings.Test)
            {
                CreateTestDir();
            }
            GetRatingsAndRename();

//            Cfg.Serialize(ConfigPath);
            Logger.Dispose();
            Console.ReadLine();

        }

        private static void CreateTestDir(bool delete = true)
        {
            Log($"Create Test Dir Set to true. Creating empty folders from {DownloadPath} in {MainDir.FullName}");
            
            if (delete)
            {
                MainDir.Delete(true);
            }
            var movs = DownloadDir.EnumerateVideoDirectories();
            Log($"Creating {movs.Count()} empty movie folders in {MainDir.FullName}");
            foreach (var mov in movs)
            {
                mov.CreateEmptyDir(MainDir);
            }
        }

        private static void GetRatingsAndRename()
        {
            Log($"Config settings:\n{Cfg.Config}");
            if (Cfg.Config.Rename)
            {
                var dirs = MainDir.GetDirectories("*IMDB*");
                foreach (var dir in dirs)
                {
                    dir.SortByName();
                }
            }
            else
            {
                RenameAndWriteInfo();
                if (Cfg.Config.Settings.MetaCritic.Add)
                {
                    var movies = new DirectoryInfo(Cfg.Config.Path).EnumerateMovies();
                    foreach (var m in movies)
                    {
                        Log($"{m.Title} - Score = {m.Score}. Meta = {m.MetaCritic}");
                    }
                }
            }
            
            
                
                //todo logic here

                /*var dirs = new DirectoryInfo(Cfg.Config.Path).GetDirectories("*IMDB*");
                foreach (var MainDir in dirs)
                {
                    string rat;
                    if (MainDir.Meta(out rat))
                    {
                        Log($"Got meta rating {rat}");
                    }
                }*/
//            }
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
            foreach (var d in dirs)//MainDir.GetDirectories().Where(x => !Cfg.Config.IgnoreDirectoires.Contains(x.Name)))
            {
                d.Rename();
//                d.WriteInfoToFile();
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

        private static void Log(string s, [CallerMemberName] string name = "", [CallerLineNumber] int ln = 0,
            [CallerFilePath] string path = "")
        {
            Logger.BLog(s, name, path, ln);
        }

        private static void Error(string s, [CallerMemberName] string name = "", [CallerLineNumber] int ln = 0,
            [CallerFilePath] string path = "")
        {
            Logger.BError(s, name, path, ln);
        }

        private static void Error(Exception ex, [CallerMemberName] string name = "",
            [CallerLineNumber] int ln = 0,
            [CallerFilePath] string path = "")
        {
            Logger.BError(ex, name, path, ln);
        }
    }
}

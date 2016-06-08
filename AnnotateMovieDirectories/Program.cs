using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AnnotateMovieDirectories.Configuration;
using AnnotateMovieDirectories.Extensions;
using AnnotateMovieDirectories.Extensions.DirInfo;
using AnnotateMovieDirectories.Logging;
using AnnotateMovieDirectories.Movies;
using AnnotateMovieDirectories.Omdb.Metacritic;

namespace AnnotateMovieDirectories
{
    class Program
    {
        public const string ConstConfigPath = @"C:\Users\Ty\Documents\Downloads\To Watch\AnnotateConfig.xml";
        public const string BackupConfigPath = "Config.xml";
        private const string DownloadPath = @"C:\Users\Ty\Documents\Downloads\To Watch";
        private static readonly DirectoryInfo DownloadDir = new DirectoryInfo(DownloadPath);

        public static string ConfigPath { get; set; }
        public static Random Rand => new Random();

        private static bool HasArgs { get; set; }

        static int Main(string[] args)
        {
            Logger.Init("Log.txt");
            Log($"Beginning annotation script");
            if (!GetConfigPath(args)) return -1;

            if (!DeserializeConfig()) return -1;
            if (!Cfg.DownloadDirExists)
            {
                Error($"Specified download directory {Cfg.Config.Path} does not exist.");
                Console.ReadLine();
                return 1;
            }

            if (Cfg.Config.Settings.Test)
            {
                CreateTestDir();
            }

            if (Cfg.Config.ForceAppendGenre)
            {
                var movies = Cfg.DownloadDir.EnumerateAnnotatedDirectories();
                foreach (var dir in movies)
                {
                    GenreTagger.AddGenreToDirectory(dir);
                }
            }

            Annotater.GetRatingsAndRename();
            if (Logger.EncounteredError)
            {
                Console.ForegroundColor=ConsoleColor.Red;
                Console.Error.WriteLine($"Encountered error while running");
                Console.ResetColor();
                return 1;
            }
//            Logger.Dispose();
//            Console.ReadLine();
            return 0;

        }

        private static bool DeserializeConfig()
        {
            if (!File.Exists(ConfigPath))
            {
                Error($"Config does not exist at path {ConfigPath}.");
                return false;
            }
            Cfg.Deserialize(ConfigPath);
            return true;
        }

        private static bool GetConfigPath(string[] args)
        {
            HasArgs = args.Any();

            ConfigPath = ConstConfigPath;
            if (HasArgs)
            {
                var configArg = args.FirstOrDefault(x => x.StartsWith("-config"));
                if (configArg.IsNull())
                {
                    Error($"Args specified but does not contain -config. ");
                    Error($"Args = {string.Join(" ", args)}");
                    return false;
                }
                string tmpConfigPath = configArg.Split('=')[1].Trim('"');
                if (!File.Exists(tmpConfigPath))
                {
                    Error($"Specified config path {tmpConfigPath} does not exist");
                    return false;
                }
                ConfigPath = tmpConfigPath;
                Log($"Config path overridden to {ConfigPath}");
            }
            return true;
        }

        private static void CreateTestDir(bool delete = true)
        {
            Log($"Create Test Dir Set to true. Creating empty folders from {DownloadPath} in {Annotater.MainDir.FullName}");
            if (delete)
            {
                Annotater.MainDir.Delete(true);
            }
            var movs = DownloadDir.EnumerateVideoDirectories();
            Log($"Creating {movs.Count()} empty movie folders in {Annotater.MainDir.FullName}");
            foreach (var mov in movs)
            {
                mov.CreateEmptyDir(Annotater.MainDir);
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

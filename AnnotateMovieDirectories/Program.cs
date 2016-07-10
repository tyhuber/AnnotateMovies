using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AnnotateMovieDirectories.Configuration;
using AnnotateMovieDirectories.Configuration.Yaml;
using AnnotateMovieDirectories.Extensions;
using AnnotateMovieDirectories.Extensions.DirInfo;
using AnnotateMovieDirectories.Logging;
using AnnotateMovieDirectories.Movies;
using AnnotateMovieDirectories.Movies.RogerEbert;
using IpaExtensions.FileSystem;
using IpaExtensions.Objects;
using YamlDotNet.Serialization;

namespace AnnotateMovieDirectories
{
    class Program
    {
        public const string ConstConfigPath = @"C:\Users\Ty\Documents\Downloads\To Watch\AnnotateConfig.xml";
        public const string BackupConfigPath = "Config.xml";
        private const string DownloadPath = @"C:\Users\Ty\Documents\Downloads\To Watch";
        private static readonly DirectoryInfo DownloadDir = new DirectoryInfo(DownloadPath);
        private static Regex LogMediaRegex1 => new Regex(@"No new movies found and no movies moved");
        private static Regex LogMediaRegex2 => new Regex(@"Wrote all \d+ movies \(\d+ new - \d+ moved\)");

        public static string ConfigPath { get; set; }
        public static Random Rand => new Random();

        private static bool HasArgs { get; set; }

        static int Main(string[] args)
        {
            Logger.Init("Log.txt");
            Log($"Beginning annotation script");
            if (!GetConfigPath(args)) return -1;

            if (!DeserializeConfig()) return -1;
            
            int main = Run();
            if (Settings.RunMediaLogger)
            {
                if (!RunMediaLogger()) return -1;
            }
            return main;
        }

        private static bool RunMediaLogger()
        {
            FileInfo exe = Settings.Config.LogMedia.Executable;
            Log($"Run media logger is set to true. Running executable {exe.FullName}");
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = exe.FullName,
                WorkingDirectory = exe.DirectoryName,
                Verb = "runas",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false
            };
            Log($"Starting process with start info:\n" +
                $"\tFilename = {startInfo.FileName}\n" +
                $"\tWorking Directory = {startInfo.WorkingDirectory}\n" +
                $"\tVerb = {startInfo.Verb}\n" +
                $"\t RedirectStandardOutput = {startInfo.RedirectStandardOutput}\n" +
                $"\t RedirectStandardError = {startInfo.RedirectStandardError}\n" +
                $"\t UseShellExecute = {startInfo.UseShellExecute}\n");
            return HandleRunningProcess(startInfo, exe);
        }

        private static bool HandleRunningProcess(ProcessStartInfo startInfo, FileInfo exe)
        {
            string stdErr;
            KillRunningProcs(exe);
            if (!RunProcess(startInfo, exe, out stdErr)) return false;
            return stdErr.IsNullOrWhitespace();
        }

        private static bool RunProcess(ProcessStartInfo startInfo, FileInfo exe, out string stdErr)
        {
            stdErr = string.Empty;
            using (var proc = Process.Start(startInfo))
            {
                if (proc == null)
                {
                    Error($"Umm somehow process is null.");
                    return false;
                }
                Log($"Started process");
                Log($"{exe.Name} StdOut:");

                stdErr = WaitForProcessCompletion(proc);
            }
            return true;
        }

        private static string WaitForProcessCompletion(Process proc)
        {
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                Console.WriteLine(line);
                if (!line.IsNullOrWhitespace() && (LogMediaRegex1.IsMatch(line) || LogMediaRegex2.IsMatch(line)))
                {
                    Log("I think it is waiting for input - lets see");
                    proc.StandardInput.WriteLine("yo");
                }
            }
           string stdErr = proc.StandardError.ReadToEnd();
            Log($"Process StdErr:");
            Log(stdErr.IsNullOrWhitespace() ? "NULL" : stdErr);
            proc.WaitForExit();
            return stdErr;
        }

        private static void KillRunningProcs(FileInfo exe)
        {
            var runningProcs = Process.GetProcessesByName(exe.NameWithoutExt());
            if (runningProcs.Any())
            {
                Log($"{runningProcs.Length} {exe.NameWithoutExt()} processes are already running. Kiling");
                foreach (var proc in runningProcs)
                {
                    proc.Kill();
                }
            }
        }

        private static void DeserializeYamlConfig()
        {
            Deserializer deserializer = new Deserializer();
            using (var reader = new StreamReader("AnnotateConfig.yaml"))
            {
                var yaml = deserializer.Deserialize<YamlConfig>(reader);
                Console.WriteLine(yaml);
            }
        }

        private static void ConvertConfig()
        {
            Serializer ser = new Serializer();
            var yaml = Settings.Config.ConvertToYaml();
            Console.WriteLine(yaml);

            using (var writer = new StreamWriter("AnnotateConfig.yaml"))
            {
                ser.Serialize(writer, yaml);
            }
        }

        private static int Run()
        {
            if (!Settings.DownloadDirExists)
            {
                Error($"Specified download directory {Settings.Config.Path} does not exist.");
                Console.ReadLine();
                return 1;
            }

            if (Settings.Config.Settings.Test)
            {
                CreateTestDir();
            }
            if (Settings.Config.AppendOscars)
            {
               /* var movies = Settings.DownloadDir.EnumerateAnnotatedDirectories().Where(x => x.HasOscars()).ToList();
                Log($"Got {movies.Count} movies with oscar wins.");
                foreach (var movie in movies)
                {
                    OscarTagger.AddOscarNotation(movie);
                }*/
            }
            if (Settings.Config.ForceAppendGenre)
            {
                var movies = Settings.DownloadDir.EnumerateAnnotatedDirectories();
                foreach (var dir in movies)
                {
                    GenreTagger.AddGenreToDirectory(dir);
                }
            }

            Annotater.GetRatingsAndRename();

            if (Settings.Config.GenreMoveOn)
            {
                GenreMover.Move();
            }
            if (Logger.EncounteredError)
            {
                Console.ForegroundColor = ConsoleColor.Red;
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
            Settings.Deserialize(ConfigPath);
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

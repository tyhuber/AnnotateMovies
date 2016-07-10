using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using AnnotateMovieDirectories.Configuration.Classes;
using AnnotateMovieDirectories.Extensions;
using AnnotateMovieDirectories.Logging;
using IpaExtensions.FileSystem;

namespace AnnotateMovieDirectories.Configuration
{
    public static class Settings 
    {
        public static Config Config { get; set; }
        private static XmlSerializer Ser => new XmlSerializer(typeof(Config));
        public static DirectoryInfo DownloadDir => new DirectoryInfo(Config.Path);
        public static bool DownloadDirExists => DownloadDir.ExistsNow();

        public static bool RunMediaLogger => Config.RunMediaLogger;

        public static bool ForceRename = true;
        
        private static bool HandleXml(string path, bool serialize,Action<string> xmlAction)
        {
            try
            {
               xmlAction.Invoke(path);
               return true;
            }
            catch (Exception e)
            {
                string log = serialize ? "serialize" : "deserialize";
                Error($"Unable to {log} {path}");
                Error(e);
                return false;
            }
        }
        
        public static bool Deserialize(string path)
        {
            if (!File.Exists(path))
            {
                Error($"Config file {path} does not exist");
                return false;
            }
            return HandleXml(path, false, x =>
            {
                using (var sReader = new StreamReader(path))
                {
                    Config = (Config) Ser.Deserialize(sReader);
                }
            });
        }

        public static bool Serialize(string path)
        {
            return HandleXml(path, true, x =>
            {
                using (var sWriter = new StreamWriter(path))
                {
                    Ser.Serialize(sWriter, Config);
                }
            });
        }

        public static void SetDefault()
        {
            Config = DefaultConfig;
        }

        private static Config DefaultConfig => new Config
        {
            Settings = new Classes.Settings
            {
                OverwriteMovieInfo = false,
                Path = @"C:\Users\Ty\Documents\Downloads\To Watch"
            },
            Ignore = new List<string>
            {
                "Title",
                "Year",
                "Runtime",
                "Metascore",
                "imdbRating",
                "imdbID",
                "tomatoMeter",
                "tomatoRating",
                "StdImdbRating,",
                "StdRtMeter",
                "StdRtRating",
                "StdRtUserMeter",
                "StdRtUserRating",
                "Rank"
            },
            Top = new List<string>
            {
                "Genre",
                "Score",
                "Plot",
                "Director",
                "Actors",
                "Language",
                "Awards"
            },
            IgnoreDirectoires = new List<string>
            {
                "Audio Books",
                "ebooks",
                "Documentaries",
                "TV",
                "Test"
            }
        };
    

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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using AnnotateMovieDirectories.Configuration;
using AnnotateMovieDirectories.Logging;
using AnnotateMovieDirectories.Omdb;

namespace AnnotateMovieDirectories.Extensions.DirInfo
{
    public static class DirMisc
    {

        public static void Log(string s, [CallerMemberName] string name = "", [CallerFilePath] string path = "", [CallerLineNumber] int ln = 0)
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
        public static void CreateEmptyDir(this DirectoryInfo dir, DirectoryInfo newParent)
        {

            bool b = Program.Rand.Next(500) %2== 1;

            if (b)
            {
                var fi = dir.GetVideo();
                if (fi == default(FileInfo)) return;
                newParent.CreateSubdirectory(fi.GetNameWithoutExt());
            }
            else
            {
                newParent.CreateSubdirectory(dir.Name);
            }
        }

        public static bool Skip(this DirectoryInfo dir)
        {
            return Cfg.Config.IgnoreDirectory(dir) || dir.Checked();
        }

        public static bool Checked(this DirectoryInfo dir)
        {
            if(!Cfg.Config.Settings.Rename)return dir.Name.Contains("IMDB") && DirRegex.ScoreRegex.IsMatch(dir.Name);
            return false;
        }
    }
}
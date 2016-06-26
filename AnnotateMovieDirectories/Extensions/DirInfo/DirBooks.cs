using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using AnnotateMovieDirectories.Logging;

namespace AnnotateMovieDirectories.Extensions.DirInfo
{
    public static class DirBooks
    {
        public static Regex AudioRegex => new Regex(@"");

        
        public static bool ContainsAudioBook(this DirectoryInfo dir)
        {
            return dir.EnumerateFiles().Any(x => x.IsAudio());
        }

        public static IEnumerable<FileInfo> EnumerateAudioFiles(this DirectoryInfo dir)
        {
            return dir.EnumerateFiles().Where(x => x.IsAudio());
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
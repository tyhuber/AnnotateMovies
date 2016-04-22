using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace AnnotateMovieDirectories.Logging
{
    public static class Logger
    {
        private static StreamWriter Writer { get; set; }
        private static StreamWriter EWriter { get; set; }
        private static string LogPath { get; set; }
        private static string ErrorPath { get; set; }

        private static bool WriterNull
        {
            get
            {
                if (Writer != null) return false;
                Console.WriteLine($"Writer is null so cannot log message");
                return true;
            }
        }

        /*
        private static void Log(string s, [CallerMemberName] string name = "", [CallerLineNumber] int ln = 0, [CallerFilePath] string path = "")
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
        */

        public static void Init(string logPath)
        {
            LogPath = logPath;
            ErrorPath = Path.Combine(Path.GetDirectoryName(logPath), $"Error{Path.GetFileName(logPath)}");
            Writer = new StreamWriter(LogPath);
        }

        public static void Log(string s, [CallerMemberName] string name = "",
            [CallerFilePath] string path = "", [CallerLineNumber] int ln = 0)
        {
            string msg = GetMsg(s, name, path, ln, LogType.Message);
            Console.WriteLine(msg);
            SafeLog(msg);
        }

        public static void BLog(string s, string name, string path, int ln, LogType lt = LogType.Message)
        {
            string msg = GetMsg(s, name, path, ln, LogType.Message);
            Console.WriteLine(msg);
            if (WriterNull) return;
            Writer.WriteLine(msg);
            Writer.Flush();
        }

        public static void BLog(string msg)
        {
            Console.WriteLine(msg);
            if (WriterNull) return;
            Writer.WriteLine(msg);
            Writer.Flush();
        }

        public static void BError(string s, string name, string path, int ln)
        {
            string msg = GetMsg(s, name, path, ln, LogType.Error);
            SafeLog(msg);
            SafeError(msg);
        }

        public static void BError(Exception ex, string name, string path, int ln)
        {
            string msg = GetMsg(ex.Message, name, path, ln, LogType.Exception);
            string stack = GetMsg(ex.StackTrace, name, path, ln, LogType.StackTrace);

            SafeLog(msg);
            SafeLog(stack);
            SafeError(msg);
            SafeError(stack);
        }

        public static void Error(string s, [CallerMemberName] string name = "",
            [CallerFilePath] string path = "", [CallerLineNumber] int ln = 0)
        {
            string msg = GetMsg(s, name, path, ln, LogType.Error);
            Console.WriteLine(msg);
            SafeLog(msg);
            SafeError(msg);
        }

        public static void Error(Exception ex, [CallerMemberName] string name = "",
            [CallerFilePath] string path = "", [CallerLineNumber] int ln = 0)
        {
            string msg = GetMsg(ex.Message, name, path, ln, LogType.Exception);
            Console.WriteLine(msg);
            SafeLog(msg);
            string stack = GetMsg(ex.StackTrace, name, path, ln, LogType.StackTrace);
            SafeLog(stack);
            SafeError(msg);
            SafeError(stack);
        }

        private static void SafeLog(string s, string name, string path, int ln, LogType lt = LogType.Blank)
        {
            if (!WriterNull)
            {
                BLog(s, name, path, ln, lt);
            }
        }

        private static void SafeLog(string msg)
        {
            if (!WriterNull)
            {
                BLog(msg);
            }
        }

        private static void SafeError(string msg)
        {
            using (EWriter = new StreamWriter(ErrorPath, true))
            {
                EWriter.WriteLine(msg);
                EWriter.Flush();
            }
        }



        private static string GetMsg(string s, string name, string path, int ln, LogType lType = LogType.Blank)
        {
            string append =
                $"{lType.Str()}[{DateTime.Now}] [{Path.GetFileNameWithoutExtension(path)}.{name} ln {ln}] - ";
            string msg = $"{append}{s}";
            return msg;
        }

        public static void Dispose()
        {
            Writer.Flush();
            Writer.Close();
            Writer.Dispose();
        }

        public enum LogType
        {
            Message,
            Warning,
            Error,
            Exception,
            StackTrace,
            Blank
        }

        private static string Str(this LogType l)
        {
            switch (l)
            {
                case LogType.Blank:
                    return string.Empty;
                case LogType.StackTrace:
                    return "[Stack Trace]";
                case LogType.Exception:
                    return "[CAUGHT EXCEPTION]";
                default:
                    return $"[{l.ToString().ToUpper()}]";
            }

        }
    }
}
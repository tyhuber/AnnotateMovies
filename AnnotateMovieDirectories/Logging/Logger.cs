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
        private static string ErrorPath => LogPath.Replace(Path.GetFileName(LogPath), "ErrorLog.txt");

        private static string WarningPath => LogPath.Replace(Path.GetFileName(LogPath),"WarningLog.txt");

        public static bool EncounteredError { get; set; }

        private static bool WriterNull
        {
            get
            {
                if (Writer != null) return false;
                Console.WriteLine($"Writer is null so cannot log message");
                return true;
            }
        }
        public static void Init(string logPath)
        {
            LogPath = logPath;
            if(File.Exists(LogPath))File.Delete(LogPath);
            if(File.Exists(ErrorPath))File.Delete(ErrorPath);
            if(File.Exists(WarningPath))File.Delete(WarningPath);
//            Writer = new StreamWriter(LogPath);
        }

        public static void Log(string s, [CallerMemberName] string name = "",
            [CallerFilePath] string path = "", [CallerLineNumber] int ln = 0)
        {
            string msg = GetMsg(s, name, path, ln, LogType.Message);
            SWrite(msg, LogType.Message);
            /*Console.WriteLine(msg);
            SafeLog(msg);*/
        }

        public static void BLog(string s, string name, string path, int ln, LogType lt = LogType.Message)
        {
            string msg = GetMsg(s, name, path, ln, LogType.Message);
            SWrite(msg, LogType.Message);
            /* Console.WriteLine(msg);
             using (Writer = new StreamWriter(LogPath,true))
             {
                 Writer.WriteLine(msg);
                 Writer.Flush();
             }*/
        }

        private static void SWrite(string msg, LogType lt)
        {
            using (var writer = new StreamWriter(lt.GetPath(),true))
            {
                writer.WriteLine(msg);
//                writer.Flush();
                writer.Close();
            }
        }

        public static void BLog(string msg, bool err = false)
        {
            if (err)
            {
                WriteConsoleError(msg);
            }
            else
            {
                Console.WriteLine(msg);
            }
            SWrite(msg,LogType.Message);
            /*using(Writer = new StreamWriter(LogPath,true))
            {
                Writer.WriteLine(msg);
                Writer.Flush();
            }*/
        }

        private static void WriteConsoleError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        public static void BError(string s, string name, string path, int ln)
        {
            string msg = GetMsg(s, name, path, ln, LogType.Error);
            SWrite(msg, LogType.Error);
            /*SafeLog(msg,true);
            SafeError(msg);*/
        }

        public static void BError(Exception ex, string name, string path, int ln)
        {
            string msg = GetMsg(ex.Message, name, path, ln, LogType.Exception);
            string stack = GetMsg(ex.StackTrace, name, path, ln, LogType.StackTrace);
            SWrite(msg, LogType.Error);
            SWrite(msg, LogType.EMsg);
            SWrite(stack, LogType.Error);
            SWrite(stack, LogType.EMsg);
            /*SafeLog(msg,true);
            SafeLog(stack,true);
            SafeError(msg);
            SafeError(stack);*/
        }

        public static void Error(string s, [CallerMemberName] string name = "",
            [CallerFilePath] string path = "", [CallerLineNumber] int ln = 0)
        {
            string msg = GetMsg(s, name, path, ln, LogType.Error);
            SWrite(msg, LogType.Error);
            SWrite(msg, LogType.EMsg);
            /*Console.WriteLine(msg);
            SafeLog(msg,true);
            SafeError(msg);*/
        }

        public static void Error(Exception ex, [CallerMemberName] string name = "",
            [CallerFilePath] string path = "", [CallerLineNumber] int ln = 0)
        {
            string msg = GetMsg(ex.Message, name, path, ln, LogType.Exception);
            string stack = GetMsg(ex.StackTrace, name, path, ln, LogType.StackTrace);
                        
            SWrite(msg, LogType.Error);
            SWrite(msg, LogType.EMsg);            
            SWrite(stack, LogType.Error);
            SWrite(stack, LogType.EMsg);

            /*Console.WriteLine(msg);
            SafeLog(msg, true);
            SafeLog(stack,true);
            SafeError(msg);
            SafeError(stack);*/
        }

        private static void SafeLog(string s, string name, string path, int ln, LogType lt = LogType.Blank)
        {
            BLog(s, name, path, ln, lt);
        }

        private static void SafeLog(string msg, bool error = false)
        {
            SWrite(msg,LogType.Error);
            SWrite(msg,LogType.EMsg);
            BLog(msg,error);
        }

        private static void SafeError(string msg)
        {
            /*using (EWriter = new StreamWriter(ErrorPath, true))
            {
                EWriter.WriteLine(msg);
                EWriter.Flush();
            }*/
        }



        private static string GetMsg(string s, string name, string path, int ln, LogType lType = LogType.Blank)
        {
            if (lType == LogType.Error || lType == LogType.Exception || lType == LogType.StackTrace)
            {
                EncounteredError = true;
            }
            string append =
                $"{lType.Str()}[{DateTime.Now}] [{Path.GetFileNameWithoutExtension(path)}.{name} ln {ln}] - ";
            string msg = $"{append}{s}";
            if (lType == LogType.Error || lType == LogType.Exception || lType == LogType.StackTrace)
            {
                WriteConsoleError(msg);
            }
            else
            {
                Console.WriteLine(msg);
            }
            return msg;
        }

//        public static void Dispose()
//        {
//            Writer.Flush();
//            Writer.Close();
//            Writer.Dispose();
//        }

        public enum LogType
        {
            Message,
            EMsg,
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
                case LogType.EMsg:
                    return "[ERROR]";
                default:
                    return $"[{l.ToString().ToUpper()}]";
            }

        }

        private static string GetPath(this LogType l)
        {
            switch (l)
            {
                case LogType.Message:
                    return LogPath;
                case LogType.Warning:
                    return WarningPath;
                case LogType.Error:
                    return ErrorPath;
                case LogType.Exception:
                    return ErrorPath;
                case LogType.StackTrace:
                    return ErrorPath;
                case LogType.Blank:
                    return LogPath;
                case LogType.EMsg:
                    return LogPath;
                default:
                    throw new ArgumentOutOfRangeException(nameof(l), l, null);
            }
        }
    }
}
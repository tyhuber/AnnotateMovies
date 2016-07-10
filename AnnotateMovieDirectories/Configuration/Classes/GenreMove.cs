using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using AnnotateMovieDirectories.Logging;

namespace AnnotateMovieDirectories.Configuration.Classes
{
    public class GenreMove
    {
        [XmlArrayItem("Genre")]
        public List<GenreDirectory> Genres { get; set; }
        [XmlAttribute]
        public bool Use { get; set; }
        [XmlAttribute]
        public bool Force { get; set; }

        public bool Contains(IEnumerable<string> genres, out DirectoryInfo dir)
        {
            var genreDir = Genres.FirstOrDefault(gd => genres.Any(genre=>gd.Name.Trim().Equals(genre.Trim(), StringComparison.OrdinalIgnoreCase)));
            Log($"Checking if GenreMove contains any of the following genres:{string.Join(",",genres)}");
            dir = null;
            bool contains = genreDir != null;
            if (contains)
            {
                Log($"Found matching genreDir {genreDir}. Returning true");
                dir=new DirectoryInfo(genreDir.Directory);
            }
            return contains;
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

    public class GenreDirectory
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Directory { get; set; }

        public override string ToString()
        {
            return $"Genre {Name}: {Directory}";
        }
    }
}
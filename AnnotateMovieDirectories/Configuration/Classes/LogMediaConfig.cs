using System.IO;
using System.Xml.Serialization;
using IpaExtensions.FileSystem;
using IpaExtensions.Objects;

namespace AnnotateMovieDirectories.Configuration.Classes
{
    public class LogMediaConfig
    {
        [XmlAttribute]
        public bool Run { get; set; }

        [XmlText]
        public string ExecutablePath { get; set; }

        [XmlIgnore]
        public FileInfo Executable => !ExecutablePath.IsNullOrWhitespace()?ExecutablePath.GetFileInfo():null;

        [XmlIgnore]
        public bool Exists => Executable?.ExistsNow()??false;


        public override string ToString()
        {
            return base.ToString();
        }

        
    }
}
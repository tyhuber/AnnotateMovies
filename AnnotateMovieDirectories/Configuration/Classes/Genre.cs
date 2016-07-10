using System.Xml.Serialization;

namespace AnnotateMovieDirectories.Configuration.Classes
{
    public class Genre:BaseConfig
    {
        [XmlAttribute]
        public bool Append { get; set; }
        [XmlAttribute]
        public bool Force { get; set; }
    }
}
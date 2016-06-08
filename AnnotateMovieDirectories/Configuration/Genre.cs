using System.Xml.Serialization;

namespace AnnotateMovieDirectories.Configuration
{
    public class Genre:BaseConfig
    {
        [XmlAttribute]
        public bool Append { get; set; }
        [XmlAttribute]
        public bool Force { get; set; }
    }
}
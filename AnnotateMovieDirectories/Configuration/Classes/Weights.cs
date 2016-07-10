using System;
using System.Xml.Serialization;
using AnnotateMovieDirectories.Configuration.Yaml;

namespace AnnotateMovieDirectories.Configuration.Classes
{
    public class Weights:BaseConfig
    {
        [XmlAttribute]
        public double Imdb { get; set; }
        [XmlAttribute]
        public double RtFresh { get; set; }
        [XmlAttribute]
        public double RtRating { get; set; }
        [XmlAttribute]
        public double MetaCritic { get; set; }
        [XmlAttribute]
        public double Ebert { get; set; }

        [XmlIgnore]
        public bool Valid => Math.Abs(Imdb + RtFresh + RtRating + MetaCritic - 1) < double.Epsilon;

        public YamlWeights ConvertToYaml()
        {
            return Convert<YamlWeights>();
        }

    }
}
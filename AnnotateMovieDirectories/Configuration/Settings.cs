using System.Collections.Generic;
using System.Xml.Serialization;
using AnnotateMovieDirectories.Configuration.Yaml;

namespace AnnotateMovieDirectories.Configuration
{
    public class Settings : BaseConfig
    {
        [XmlAttribute]
        public string Path { get; set; }
        [XmlAttribute]
        public bool OverwriteMovieInfo { get; set; }
        [XmlAttribute]
        public bool Test { get; set; }
        [XmlAttribute]
        public bool Rename { get; set; }

        [XmlElement]
//        [IgnoreConvert]
        public MetaCritic MetaCritic { get; set; }
        [XmlElement]
//        [IgnoreConvert]
        public Weights Weights { get; set; }

        public YamlSettings ConvertToYaml()
        {
            YamlSettings yaml = Convert<YamlSettings>();
            yaml.MetaCritic = MetaCritic.Convert<YamlMetaCritic>();
            yaml.Weights = Weights.Convert<YamlWeights>();
            return yaml;
        }



    }
}
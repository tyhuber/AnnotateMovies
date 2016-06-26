using System.Xml.Serialization;
using AnnotateMovieDirectories.Configuration.Yaml;

namespace AnnotateMovieDirectories.Configuration
{
    public class MetaCritic:BaseConfig
    {
        [XmlAttribute]
        public bool Add { get; set; }
        [XmlAttribute]
        public bool Use { get; set; }

        public YamlMetaCritic ConvertToYaml()
        {
            return Convert<YamlMetaCritic>();
        }

        /*public static implicit operator YamlMetaCritic(MetaCritic meta)
        {
            return new YamlMetaCritic
            {
                Add = meta.Add,
                Use = meta.Use
            };
        }*/
    }
}
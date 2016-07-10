using AnnotateMovieDirectories.Configuration.Classes;
using YamlDotNet.Serialization;

namespace AnnotateMovieDirectories.Configuration.Yaml
{
    public class YamlGenre : BaseConfig
    {
        [YamlMember]
        public bool Append { get; set; }
        [YamlMember]
        public bool Force { get; set; }

    }
}
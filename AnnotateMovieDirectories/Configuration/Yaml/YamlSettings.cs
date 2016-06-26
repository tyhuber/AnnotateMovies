using YamlDotNet.Serialization;

namespace AnnotateMovieDirectories.Configuration.Yaml
{
    public class YamlSettings : BaseConfig
    {

        [YamlMember]
        public string Path { get; set; }

        [YamlMember]
        public bool OverwriteMovieInfo { get; set; }

        [YamlMember]
        public bool Test { get; set; }

        [YamlMember]
        public bool Rename { get; set; }

        [YamlMember]
        public YamlMetaCritic MetaCritic { get; set; }

        [YamlMember]
        public YamlWeights Weights { get; set; }
    }
}
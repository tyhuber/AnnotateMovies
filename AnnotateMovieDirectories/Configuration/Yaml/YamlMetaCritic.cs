using YamlDotNet.Serialization;

namespace AnnotateMovieDirectories.Configuration.Yaml
{
    public class YamlMetaCritic : BaseConfig
    {
        [YamlMember]
        public bool Add { get; set; }
        [YamlMember]
        public bool Use { get; set; }
    }
}
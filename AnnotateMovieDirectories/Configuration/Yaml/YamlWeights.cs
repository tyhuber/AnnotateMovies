using System;
using YamlDotNet.Serialization;

namespace AnnotateMovieDirectories.Configuration.Yaml
{
    public class YamlWeights:BaseConfig
    {

        [YamlMember]
        public double Imdb { get; set; }

        [YamlMember]
        public double RtFresh { get; set; }

        [YamlMember]
        public double RtRating { get; set; }

        [YamlMember]
        public double MetaCritic { get; set; }

        [YamlIgnore]
        public bool Valid => Math.Abs(Imdb + RtFresh + RtRating + MetaCritic - 1) < double.Epsilon;

        
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AnnotateMovieDirectories.Movies;
using YamlDotNet.Serialization;

namespace AnnotateMovieDirectories.Configuration.Yaml
{
    public class YamlConfig:BaseConfig
    {

        [YamlIgnore]
        public string Path => Settings.Path;

        [YamlIgnore]
        public bool Overwrite => Settings.OverwriteMovieInfo;

        [YamlMember]
        public YamlSettings Settings { get; set; }

        [YamlMember]
        public YamlGenre Genre { get; set; }

        [YamlMember]
        public List<string> Top { get; set; }

        [YamlMember]
        public List<string> Ignore { get; set; }

        [YamlMember]
        public RenameBy RenameBy { get; set; }

        [YamlIgnore]
        public bool Rename => RenameBy != RenameBy.None;

        [YamlIgnore]
        public List<string> AllIgnore => Top.Concat(Ignore).ToList();

        [YamlIgnore]
        public bool AppendGenre => Genre != null && Genre.Append;

        [YamlIgnore]
        public bool ForceAppendGenre => AppendGenre && Genre.Force;

        [YamlMember]
        public List<string> IgnoreDirectoires { get; set; }


        public double GetWeightedScore(Movie movie)
        {
            double rating = 0;
            double weight = 0;
            foreach (RatingType type in Enum.GetValues(typeof(RatingType)))
            {
                AddWeight(movie, type, ref rating, ref weight);
            }
            if (Math.Abs(weight) < double.Epsilon) return 0;
            return Math.Round(rating / weight, 1);
        }

        private void AddWeight(Movie movie, RatingType type, ref double rating, ref double weight)
        {
            double tmpRating;
            if (!movie.GetRating(type, out tmpRating))
            {
                Log($"Could not get {type} for {movie.Title}");
                return;
            }
            if (Math.Abs(tmpRating) < Double.Epsilon)
            {
                Log($"{type} == {tmpRating}. Will not factor into rating");
                return;
            }
            double tmpWeight = GetWeight(type);
            //            Log($"{movie.Title}-{type} = {tmpRating}. Weight = {tmpWeight}");
            rating += tmpRating * tmpWeight;
            weight += tmpWeight;
        }

        public double GetWeight(RatingType type)
        {
            switch (type)
            {
                case RatingType.Imdb:
                    return Settings.Weights.Imdb;
                case RatingType.RtFresh:
                    return Settings.Weights.RtFresh;
                case RatingType.RtRating:
                    return Settings.Weights.RtRating;
                case RatingType.MetaCritic:
                    return Settings.Weights.MetaCritic;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public bool IgnoreDirectory(DirectoryInfo dir)
        {
            return IgnoreDirectoires.Contains(dir.Name);
        }

        public override string ToString()
        {
            return string.Join("\n", Reflect());
        }
    }
}
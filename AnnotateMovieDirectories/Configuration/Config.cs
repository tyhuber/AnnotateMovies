using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using AnnotateMovieDirectories.Configuration.Classes;
using AnnotateMovieDirectories.Configuration.Yaml;
using AnnotateMovieDirectories.Extensions;
using AnnotateMovieDirectories.Logging;
using AnnotateMovieDirectories.Movies;

namespace AnnotateMovieDirectories.Configuration
{
    [XmlRoot(Namespace = "")]
    public class Config:BaseConfig
    {
        [XmlIgnore]
        public string Path => Settings.Path;

        [XmlIgnore]
        public bool Overwrite => Settings.OverwriteMovieInfo;
        [XmlElement]
        public Classes.Settings Settings { get; set; }
        [XmlElement(IsNullable = true)]
        public Genre Genre { get; set; }
        [XmlArrayItem("Property")]
        public List<string> Top { get; set; }
        [XmlArrayItem("Property")]
        public List<string> Ignore { get; set; }

        [XmlElement(IsNullable = true)]
        public LogMediaConfig LogMedia { get; set; }

        [XmlAttribute]
        public RenameBy RenameBy { get; set; }

        [XmlAttribute]
        public bool AppendOscars { get; set; }

        [XmlIgnore]
        public bool Rename => RenameBy != RenameBy.None;
        [XmlIgnore]
        public List<string> AllIgnore => Top.Concat(Ignore).ToList();

        [XmlIgnore]
        public bool AppendGenre => Genre != null && Genre.Append;

        [XmlIgnore]
        public bool ForceAppendGenre => AppendGenre && Genre.Force;

        [XmlIgnore]
        public bool RunMediaLogger => LogMedia != null && LogMedia.Run && LogMedia.Exists;


        [XmlArray("Name")]
        public List<string> IgnoreDirectoires { get; set; }

        [XmlElement(IsNullable = true)]
        public GenreMove MoveGenres { get; set; }
        [XmlIgnore]
        public bool GenreMoveOn => MoveGenres != null && MoveGenres.Use;
        [XmlIgnore]
        public bool GenreMoveForce => GenreMoveOn && MoveGenres.Force;

        public YamlConfig ConvertToYaml()
        {
            YamlConfig yaml = Convert<YamlConfig>();
            yaml.Settings = Settings.ConvertToYaml();
            yaml.Genre = Genre.Convert<YamlGenre>();
            return yaml;
        }


        public double GetWeightedScore(Movie movie)
        {
            double rating = 0;
            double weight = 0;
            foreach (RatingType type in Enum.GetValues(typeof(RatingType)))
            {
                AddWeight(movie,type,ref rating,ref weight);
            }
            if (Math.Abs(weight) < double.Epsilon) return 0;
            return Math.Round(rating/weight,1);
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
            rating += tmpRating*tmpWeight;
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
                case RatingType.Ebert:
                    return Settings.Weights.Ebert;
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


    public enum RatingType
    {
        Imdb,
        RtFresh,
        RtRating,
        MetaCritic,
        Ebert
    }

    public enum RenameBy
    {
        Score,
        Runtime,
        Year,
        Normal,
        None
    }
}
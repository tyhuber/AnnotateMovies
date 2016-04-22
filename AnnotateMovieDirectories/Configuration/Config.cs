using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using AnnotateMovieDirectories.Extensions;
using AnnotateMovieDirectories.Logging;
using AnnotateMovieDirectories.Tools;

namespace AnnotateMovieDirectories.Configuration
{
    [XmlRoot(Namespace = "")]
    public class Config:BaseConfig
    {
        [XmlIgnore]
        public string Path => Settings.Path;

        [XmlIgnore]
        public bool Overwrite => Settings.Overwrite;
        [XmlElement]
        public Settings Settings { get; set; }
        [XmlArrayItem("Property")]
        public List<string> Top { get; set; }
        [XmlArrayItem("Property")]
        public List<string> Ignore { get; set; }

        [XmlAttribute]
        public RenameBy RenameBy { get; set; }

        [XmlIgnore]
        public bool Rename => RenameBy != RenameBy.None;
        [XmlIgnore]
        public List<string> AllIgnore => Top.Concat(Ignore).ToList();


        [XmlArray("Name")]
        public List<string> IgnoreDirectoires { get; set; }


        public double GetWeightedScore(Movie movie)
        {
            double rating = 0;
            double weight = 0;
            foreach (RatingType type in Enum.GetValues(typeof(RatingType)))
            {
                AddWeight(movie,type,ref rating,ref weight);
//                Log($"{movie.Title} - Type-{type}. Sum rating = {rating}. Sum weight = {weight}");
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

        /*private List<string> Reflect()
        {
            var props = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x=>!x.IsDefined(typeof(XmlIgnoreAttribute)));
            List<string> nameVals = new List<string>();
            foreach (var p in props)
            {
                if (ReflectIEnumerable(p, ref nameVals)) continue;
//                BLog($"{p.Name}-{p.GetValue(this,null)}");
                  nameVals.Add($"{p.Name}: {p.GetValue(this, null)}");
            }
            return nameVals;
        }

        private bool ReflectIEnumerable(PropertyInfo p, ref List<string> nameVals)
        {
            if (p.PropertyType == typeof(string) ||
                !p.PropertyType.GetInterfaces().Contains(typeof(IEnumerable))) return false;
            nameVals.Add($"{p.Name}:");
            foreach (var s in (IEnumerable) p.GetValue(this, null))
            {
                nameVals.Add($"  -{s}");
            }
            return true;
        }

        private static void Log(string s, [CallerMemberName] string name = "", [CallerLineNumber] int ln = 0,
            [CallerFilePath] string path = "")
        {
            Logger.BLog(s, name, path, ln);
        }

        private static void BLog(string s)
        {
            Logger.BLog(s);
        }

        private static void Error(string s, [CallerMemberName] string name = "", [CallerLineNumber] int ln = 0,
            [CallerFilePath] string path = "")
        {
            Logger.BError(s, name, path, ln);
        }*/
    }


    public enum RatingType
    {
        Imdb,
        RtFresh,
        RtRating,
        MetaCritic
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
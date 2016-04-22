using System;
using AnnotateMovieDirectories.Configuration;

namespace AnnotateMovieDirectories.Attributes
{
    [AttributeUsage(AttributeTargets.Class |
AttributeTargets.Field |
AttributeTargets.Property,
AllowMultiple = true)]
    public class Tag:Attribute
    {
        public string Name { get; }
        

        public Tag(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Field |
                    AttributeTargets.Property)]
    public class Rating : Attribute
    {
        public RatingType Type { get; }

        public double Weight { get; }
        public double RatingWeighted { get; set; }
        public Rating(RatingType type)
        {
            Type = type;
            Weight = Cfg.Config.GetWeight(type);
        }

        public double WeightedRating(double rating)
        {
            RatingWeighted= Weight*rating;
            return RatingWeighted;
        }

        public override string ToString()
        {
            return $"Type {Type}, Weight - {Weight}, Weighted Rating = {RatingWeighted}";
        }
    }
}
using System;
using System.Collections.Generic;
using AnnotateMovieDirectories.Configuration;
using AnnotateMovieDirectories.Extensions;

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
        
        
        public bool WeightedRating(double rating, out double weightedRating)
        {
            weightedRating = 0;
            if (rating.IsZero())
            {
                return false;
            }
            RatingWeighted= Weight*rating;
            weightedRating = RatingWeighted;
            return true;
        }

        public override string ToString()
        {
            return $"Type {Type}, Weight - {Weight}, Weighted Rating = {RatingWeighted}";
        }
    }

    public class WeightedRating
    {
        public double Weight => Rating.Weight;
        public double WeightRating => Weight*UnweightedRating; 

        private double UnweightedRating => Kv.Value;
        public bool Valid => !UnweightedRating.IsZero();
        public Rating Rating => Kv.Key;
        private RatingType Type => Rating.Type;
        private KeyValuePair<Rating,double> Kv { get;}

        public WeightedRating(KeyValuePair<Rating, double> kv)
        {
            Kv = kv;
        }

        public override string ToString()
        {
            return $"{Type}: Valid? {Valid}. Weighted={WeightRating.Round()} Unweighted={UnweightedRating.Round()} Weight={Weight.Round()}. ";
        }
    }
}
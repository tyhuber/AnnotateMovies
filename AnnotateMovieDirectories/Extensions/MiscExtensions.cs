using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using AnnotateMovieDirectories.Attributes;
using AnnotateMovieDirectories.Configuration;
using AnnotateMovieDirectories.Logging;
using AnnotateMovieDirectories.Movies.Metacritic;

namespace AnnotateMovieDirectories.Extensions
{
    public static class MiscExtensions
    {
        public static bool IsDefault<T>(this T t)
        {
            return t.Equals(default(T));
        }

        public static bool TryGetMatch(this Regex reg, string s, out Match match)
        {
            match = default(Match);
            if (!reg.IsMatch(s)) return false;
            match = reg.Match(s);
            return true;
        }
        public static string ReplaceWithString(this Regex reg, string s, string replace = "")
        {
            if (!reg.IsMatch(s)) return s;
            return reg.Replace(s, replace);
        }

        public static bool TryGetMeta(this string name, string year, out double tmp)
        {
            string ratingString = Meta.Query(name, year);
            tmp = 0;
            if (string.IsNullOrWhiteSpace(ratingString))
            {
                Error($"Could not get meta rating for {name} ({year})");
                return false;
            }
            if (!double.TryParse(ratingString, out tmp))
            {
                Error($"Unable to parse meta rating string {ratingString}");
                return false;
            }
            return true;
        }

        public static string ReplaceWithString(this string pattern, string s, string replace = "")
        {
            if (!s.Contains(pattern)) return s;
            return s.Replace(pattern, replace);
        }


        public static double Round(this double d, int decimals = 1)
        {
            return Math.Round(d, decimals);
        }

      /*  public static bool GetWeightedRating(this RatingType type, Movie mov, out double rating)
        {
            Log($"Getting weighted rating for type {type} of movie {mov.Title}");
            rating = 0;
            if (!mov.Valid)
            {
                Error($"Movie {mov.Title??"null"} is not valid. Cannot get weighted rating");
                return false;
            }
            var prop =
                mov.GetType().GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public).FirstOrDefault(
                    x => x.Name.Equals(type.ToString()));
            var weightProp =
                Settings.Config.Settings.GetType().GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public)
                   .FirstOrDefault(x => x.Name.Equals(type.ToString()));
            if (prop == null || weightProp == null)
            {
                Error($"Unable to get weight or rating property for type {type} and movie {mov.Title}");
                return false;
            }
            try
            {
                double unweightedRating = Convert.ToDouble(prop.GetValue(mov, null));
                double weight = Convert.ToDouble(weightProp.GetValue(Settings.Config.Settings, null));
                if (weight.IsZero() || unweightedRating.IsZero()) return false;
                rating = unweightedRating*weight;

                return true;
            }
            catch (Exception ex)
            {
                Error($"Caught exception trying to get rating and weight for {mov} and type {type}");
                Error(ex);
                return false;
            }
        }*/

        public static bool IsZero(this double dbl)
        {
            return Math.Abs(dbl) < Double.Epsilon;
        }
        public static bool IsZero(this int i)
        {
            return i==0;
        }

        public static bool IsNull(this string s)
        {
            return String.IsNullOrWhiteSpace(s);
        }

        private static void Log(string s, [CallerMemberName] string name = "", [CallerLineNumber] int ln = 0,
            [CallerFilePath] string path = "")
        {
            Logger.BLog(s, name, path, ln);
        }

        private static void Error(string s, [CallerMemberName] string name = "", [CallerLineNumber] int ln = 0,
            [CallerFilePath] string path = "")
        {
            Logger.BError(s, name, path, ln);
        }

        private static void Error(Exception ex, [CallerMemberName] string name = "",
            [CallerLineNumber] int ln = 0,
            [CallerFilePath] string path = "")
        {
            Logger.BError(ex, name, path, ln);
        }
    }
}
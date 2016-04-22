using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using AnnotateMovieDirectories.Attributes;
using AnnotateMovieDirectories.Configuration;
using AnnotateMovieDirectories.Logging;
using AnnotateMovieDirectories.Omdb.Metacritic;
using AnnotateMovieDirectories.Tools;
using RestSharp.Extensions;

namespace AnnotateMovieDirectories.Omdb
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class OmdbResult
    {
        public string Title { get; set; } //Title, Year, Runtime, Metascore, imdbRating, imdbID, tomatoMeter
        public string Year { get; set; }

        public string Runtime { get; set; }
        public string Genre { get; set; }
        public string Director { get; set; }

        public string Plot { get; set; }

        [Rating(RatingType.MetaCritic)]
        public double Metascore => Meta.GetRating(Title, Year);
        
        public float imdbRating { get; set; }

        public string imdbID { get; set; }
        
        public string tomatoMeter { get; set; }
        
        public string tomatoRating { get; set; }

        public string tomatoUserMeter { get; set; }
        public string tomatoUserRating { get; set; }
              
        public string Writer { get; set; }
        public string Actors { get; set; } //Actors, Language, Country, 
        public string Language { get; set; }
        public string Country { get; set; }
        public string Awards { get; set; }
        public string tomatoURL { get; set; }
        /* Unnecessary              
                    public string Poster { get; set; }
                     public string imdbVotes { get; set; }
                     public string Type { get; set; }
                     public string Rated { get; set; }
                     public string Released { get; set; }
                     public string tomatoImage { get; set; }
                     public string tomatoReviews { get; set; }
                     public string tomatoFresh { get; set; }
                     public string tomatoRotten { get; set; }
                     public string tomatoConsensus { get; set; }
                     public string tomatoUserReviews { get; set; }
                     
                     public string DVD { get; set; }
                     public string BoxOffice { get; set; }
                     public string Production { get; set; }
                     public string Website { get; set; }
                     public string Response { get; set; }
                     */
        [Rating(RatingType.Imdb)]
        public double StdImdbRating => (imdbRating) * 10;
        [Rating(RatingType.RtFresh)]
        public double? StdRtMeter => TryParse(tomatoMeter);

        private static double? TryParse(string toParse, double mult = 1)
        {
            double tmp;
            if (!double.TryParse(toParse, out tmp))
            {
                return null;
            }

            return tmp * mult;
        }
        [Rating(RatingType.RtRating)]
        public double? StdRtRating => TryParse(tomatoRating, 10);//tomatoRating*10;
        
        public double? StdRtUserMeter => TryParse(tomatoUserMeter);
        
        public double? StdRtUserRating => TryParse(tomatoUserRating, 10);

        public int Rank { get; set; }

        public double Score => GetScore();
        /*{
            get
            {
                if (!StdRtMeter.HasValue || !StdRtRating.HasValue)
                {
                    return StdImdbRating;
                }
                return StdImdbRating * .5 + StdRtMeter.Value * .3 + StdRtRating.Value * .2;
            }
        }
       */


        public string ImdbLink => $"http://www.imdb.com/title/tt0052520/{imdbID}";

        public override string ToString()
        {
            return $"{Title} ({Year}). Score = {Score}\n" +
                   $"Std:\n" +
                   $"  IMDB={StdImdbRating}\n" +
                   $"  Metacritic={Metascore}\n" +
                   $"  Rotten Tomatoes: {StdRtMeter}%, Rating={StdRtRating}";
        }

        public void WritePlotToFile(DirectoryInfo dir)
        {
            string filePath = Path.Combine(dir.FullName, "MovieInfo.txt");
            if (File.Exists(filePath))
            {
                if (Cfg.Config.Overwrite)
                {
                    File.Delete(filePath);
                }
                else
                {
                    Log($"{filePath} already exists. Won't write to file.");
                    return;
                }
            }
//            List<string> ignore = new List<string>
//            {
//                "Title",
//                "Year",
//                "Genre",
//                "Director",
//                "Plot",
//                "Runtime",
//                "Metascore",
//                "imdbRating",
//                "imdbID",
//                "tomatoMeter",
//                "tomatoRating",
//                "StdImdbRating,",
//                "StdRtMeter",
//                "StdRtRating",
//                "StdRtUserMeter",
//                "StdRtUserRating",
//                "Rank"
//            };
//
//            List<string> top = new List<string>
//            {
//                "Genre",
//                "Plot",
//                "Director",
//                "Actors",
//                "Language",
//                "Awards"
//            };
            var propInfo = GetType().GetProperties(BindingFlags.Public 
                | BindingFlags.NonPublic 
                | BindingFlags.Static 
                | BindingFlags.Instance 
                | BindingFlags.FlattenHierarchy 
                | BindingFlags.IgnoreCase);
            var topProp =
                propInfo.Where(x => Cfg.Config.Top.Any(y => y.Equals(x.Name, StringComparison.OrdinalIgnoreCase)));
            List<string> lines = new List<string>();
            foreach (var prop in topProp)
            {
                var val = Convert.ChangeType(prop.GetValue(this, null), prop.PropertyType);
//                var val = (string) prop.GetValue(this, null);
                lines.Add($"{prop.Name}: {val}");
            }
            Log("Got the following top property values:");
            Logger.BLog(string.Join("\n",lines));
            lines.Add("");
            var others = propInfo.Where(x => !Cfg.Config.AllIgnore.Any(n=>x.Name.Equals(n,StringComparison.OrdinalIgnoreCase)));
            foreach (var prop in others)
            {
                var val = Convert.ChangeType(prop.GetValue(this, null), prop.PropertyType);
                lines.Add($"{prop.Name}: {val}");
            }
            
            Log("Writing values to file:");
            Logger.BLog(string.Join("\n", lines));
            File.WriteAllLines(filePath,lines);
        }
        private double GetScore()
        {
            var props = GetType().GetProperties()
                                .Where(x =>
                                    x.GetCustomAttributes<Rating>().Any());
             var dict = props.ToDictionary(x=>x.GetCustomAttribute<Rating>(), x =>
             {
                 if (x.PropertyType.GetInterfaces().Contains(typeof(Nullable)))
                 {
                     var tmp = (double?) Convert.ChangeType(x.GetValue(this, null), x.PropertyType);
                     return tmp.Value;
                 }
                 return Convert.ToDouble(x.GetValue(this, null));
             });
//            Log($"Calculating weight for {Title}");
            foreach (var kv in dict)
            {
//                Log($"Weighted rating = {kv.Value}. {kv.Key}");
            }
            return dict.Sum(x => x.Key.WeightedRating(x.Value));

        }

        public string GetDirectoryName()
        {
            return $"{Math.Round(Score,1)}, {Title} ({Year}) IMDB-{imdbRating}. RT={Math.Round(StdRtMeter??0,1)}%.{Math.Round(StdRtRating ?? 0,1)}, Meta-{Math.Round(Metascore,1)}";
        }
        private static void Log(string s, [CallerMemberName] string name = "", [CallerLineNumber] int ln = 0,
            [CallerFilePath] string path = "")
        {
            Logger.BLog(s, name, path, ln);
        }
    }
}
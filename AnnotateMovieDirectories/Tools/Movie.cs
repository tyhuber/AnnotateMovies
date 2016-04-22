using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using AnnotateMovieDirectories.Attributes;
using AnnotateMovieDirectories.Configuration;
using AnnotateMovieDirectories.Extensions;
using AnnotateMovieDirectories.Logging;
using AnnotateMovieDirectories.Omdb;
using AnnotateMovieDirectories.Omdb.Metacritic;

namespace AnnotateMovieDirectories.Tools
{
    public class Movie
    {
        private static Regex AnnotationRegex => new Regex(@"(?<title>.*)\((?<year>(19|20)\d{2})\)\s+\[(?<time>\d+) min\]\sIMDB-(?<imdb>[1-9](\.\d)?)\s?(?<rt>RT-(?<rtfresh>\d{2,3})%\s\((?<rtrating>\d\.\d)\))?Meta-.*");
        [Tag("MovieRegex")]
        private static Regex YearRegex =>new Regex(@"\((?<year>(19|20)\d{2})\)");
        [Tag("MovieRegex")]
        private static Regex TimeRegex => new Regex(@"\[(?<time>\d+) min\]");
        [Tag("MovieRegex")]
        private static Regex ScoreRegex => new Regex(@"^(\d{1,2}\.\d),");
        [Tag("MovieRegex")]
        private static Regex OtherRegex => new Regex(@"IMDB-(?<imdb>[1-9]\.\d)\s?(?<rt>RT-(?<rtfresh>\d{2})%\s\((?<rtrating>\d\.\d)\))?\sMeta-\d+");
        [Tag("MovieRegex")]
        protected static Regex MetaRegex => new Regex(@"Meta-(?<meta>\d+)");

        public string Title { get; set; }
        public string Year { get; set; }
        public string RunTime { get; set; }
        public double Imdb { get; set; }
        public double RtFresh { get; set; }
        public double RtRating { get; set; }

        public double MetaCritic { get; set; }
        public bool Annotated { get; set; }

        public string Name { get; set; }
        public DirectoryInfo Dir { get; set; }

        public double Score => Cfg.Config.GetWeightedScore(this);

        public bool Valid
            =>
                !Title.IsNull() && !Imdb.IsZero() &&
                (!RtFresh.IsZero() || !RtRating.IsZero() || !MetaCritic.IsZero());

        private IEnumerable<Regex> GetMovieRegexes()
        {
            return GetType().GetProperties()
                .Where( x =>
                    x.GetCustomAttributes<Tag>()
                        .Any(t => t.Name.Equals("MovieRegex"))).ToList().Select(x=>Convert.ChangeType(x.GetValue(this,null),x.PropertyType) as Regex);
        }


        public Movie(string name, string year)
        {
            var omdb = OmdbSearch.Query(name, year);
        }

        public Movie(DirectoryInfo dir)
        {
            Dir = dir;
            Log($"Getting movie for {dir.Name}");
            Name = ScoreRegex.ReplaceWithString(dir.Name);
            Annotated = AnnotationRegex.IsMatch(dir.Name);
            if (ParseUnannotated(dir)) return;
            ParseAnnotated(dir);
        }

        public Movie(OmdbResult omdb)
        {
            FromOmdb(omdb);
        }

        private bool ParseUnannotated(DirectoryInfo dir)
        {
            if (!Annotated)
            {
                OmdbResult ombd;
                if (!dir.Search(out ombd))
                {
                    Error($"Unable to get omdb result for {dir.Name}");
                    return true;
                }
                FromOmdb(ombd);
            }
            ;
            return false;
        }

        public void FromOmdb(OmdbResult omdb)
        {
            Title = omdb.Title;
            Year = omdb.Year;
            double tmpRtMeter;
            if (double.TryParse(omdb.tomatoMeter, out tmpRtMeter))
            {
                RtFresh = tmpRtMeter;
            }
            double tmpRtRating;
            if (double.TryParse(omdb.tomatoRating, out tmpRtRating))
            {
                RtRating = tmpRtMeter;
            }
            double tmpMeta;
            if (Title.TryGetMeta(Year, out tmpMeta))
            {
                MetaCritic = tmpMeta;
            }
            

        }

        private void ParseAnnotated(DirectoryInfo dir)
        {
            if (ScoreRegex.IsMatch(dir.Name))
            {
                Match mYear;
                if (!YearRegex.TryGetMatch(dir.Name, out mYear))
                {
                    Error($"No year match found for {dir.Name}");
                    return;
                }
                Year = mYear.Groups[1].Value;

//                Year = MatchInt(YearRegex, true);
                Match mTime;
                if (!TimeRegex.TryGetMatch(dir.Name, out mTime))
                {
                    Error($"No year match found for {dir.Name}");
                    return;
                }
                RunTime = mTime.Value;
//                RunTime = TimeRegex.Match(Name).Value;
                
                ParseRatings();
                Regex spaceReg = new Regex(@"\s+");
                string tmpTitle =
                    Name.ReplaceWithString(Year).ReplaceWithString(RunTime);
                tmpTitle=tmpTitle.ReplaceWithString(OtherRegex.Match(tmpTitle).Value);
                Title = spaceReg.ReplaceWithString(tmpTitle, " ");
                return;
            }
            ParseStd(dir);
            GetMetaWithoutScore(dir);
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

        private void GetMetaWithoutScore(DirectoryInfo dir)
        {
            if (!GetMeta())
            {
                double tmp;
                if (dir.Meta(out tmp))
                {
                    MetaCritic = tmp;
                }
            }
        }

        private bool GetMeta()
        {
            Match m;
            if (MetaRegex.TryGetMatch(Name, out m))
            {
                double mDbl;
                if (double.TryParse(m.Groups["meta"].Value, out mDbl))
                {
                    MetaCritic = mDbl;
                    return true;
                }
            }
            return false;
        }

        public override string ToString()
        {
            return $"{Title} ({Year}) ";
        }

        private void ParseStd(DirectoryInfo dir)
        {
            var match = AnnotationRegex.Match(dir.Name);
            var groups = match.Groups;
            Title = groups["title"].Value;
            Match mYear;
            if (!YearRegex.TryGetMatch(dir.Name, out mYear))
            {
                Error($"No year match found for {dir.Name}");
                return;
            }
            Year = mYear.Groups[1].Value;//groups["year"].Value;
            RunTime = match.Groups["time"].Value;
            ParseRatings();
            
        }



        public bool GetRating(RatingType type, out double rating)
        {
            rating = 0;
            switch (type)
            {
                case RatingType.Imdb:
                    if (Imdb.IsDefault()) return false;
                    rating = Imdb*10;
                    return true;
                case RatingType.RtFresh:
                    if (RtFresh.IsDefault()) return false;
                    rating = RtFresh;
                    return true;
                case RatingType.RtRating:
                    if (RtRating.IsDefault()) return false;
                    rating = RtRating*10;
                    return true;
                case RatingType.MetaCritic:
                    if (Math.Abs(MetaCritic) < double.Epsilon) return false;
                    rating = MetaCritic;
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private void ParseRatings()
        {
            var match = OtherRegex.Match(Name);
            Imdb = GetDouble(match, "imdb");
            RtFresh = GetInt(match, "rtfresh");
            RtRating = GetDouble(match, "rtrating");
            GetMeta();
            //Todo fix meta critic ratings here.

            //            MetaCritic = GetDouble(match, "meta");
        }

        private int MatchInt(Regex reg, bool g = false)
        {
            if (!reg.IsMatch(Name))
            {
                return 0;
            }
            int tmp = 0;
            int.TryParse(g ? reg.Match(Name).Value : reg.Match(Name).Groups[1].Value, out tmp);
            return tmp;
        }

        private double MatchDouble(Regex reg, bool g = false)
        {
            if (!reg.IsMatch(Name))
            {
                return 0;
            }
            double tmp = 0;
            double.TryParse(g ? reg.Match(Name).Value : reg.Match(Name).Groups[1].Value, out tmp);
            return tmp;
        }

        public string ScoreTitle => $"'{Math.Round(Score, 1)}' {Title} ({Year}) [{RunTime}] IMDB-{Imdb} RT-{RtFresh}% ({RtRating})";
        public string RunTimeTitle => $"[{RunTime}] ({Math.Round(Score, 1)}) {Title} ({Year}) IMDB-{Imdb} RT-{RtFresh}% ({RtRating})";
        public string YearTitle => $" ({Year}) ({Math.Round(Score, 1)}) {Title} [{RunTime}] IMDB-{Imdb} RT-{RtFresh}% ({RtRating})";
        public string NormalTitle => $"{Title} ({Year}) [{RunTime}] '{Math.Round(Score, 1)}' IMDB-{Imdb} RT-{RtFresh}% ({RtRating})";

        private static int GetInt(Match match, string gname)
        {
            int rtFresh = default(int);
            if (match.Groups[gname] != null)
            {
                int.TryParse(match.Groups[gname].Value, out rtFresh);
            }
            return rtFresh;
        }

        private static double GetDouble(Match match, string gname)
        {
            double rtFresh = default(double);
            if (match.Groups[gname] != null)
            {
                double.TryParse(match.Groups[gname].Value, out rtFresh);
            }
            return rtFresh;
        }
    }
}
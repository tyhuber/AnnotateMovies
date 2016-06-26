using System;
using System.Runtime.CompilerServices;
using AnnotateMovieDirectories.Logging;
using HtmlAgilityPack;

namespace AnnotateMovieDirectories.Movies.Metacritic
{
    public static class Meta
    {
        public static double GetRating(string title, string year)
        {
            string ratingString;
            double rating;
            if (!Query(title, year, out ratingString)&&double.TryParse(ratingString,out rating))
            {
                return rating;
            }
            return 0;
        }
        public static string Query(string title, string year)
        {
            title = title.Replace(" ", "+");
            string url =
                $"http://www.metacritic.com/search/movies/{title}/results?cats%5Bmovie%5D=1&date_range_from=1995&year={year}&search_type=advanced&sort=score";
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            var node =
                doc.DocumentNode.SelectSingleNode(
                    "//span[@class=\"metascore_w medium movie positive\"]");//"//*[@id=\"main\"]/div[2]/div[1]/ul/li/div[2]/div/div[1]/span");
            string rating = node?.InnerText;//.InnerText;//
//            Log($"Got rating {rating}");
            return rating;
        }
        public static bool Query(string title, string year, out string ratingString)
        {
            title = title.Replace(" ", "+");
            string url =
                $"http://www.metacritic.com/search/movies/{title}/results?cats%5Bmovie%5D=1&date_range_from=1995&year={year}&search_type=advanced&sort=score";
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            var node =
                doc.DocumentNode.SelectSingleNode(
                    "//span[@class=\"metascore_w medium movie positive\"]");//"//*[@id=\"main\"]/div[2]/div[1]/ul/li/div[2]/div/div[1]/span");
            ratingString= node?.InnerText;//.InnerText;//
//            Log($"Got rating {ratingString}");
            return string.IsNullOrWhiteSpace(ratingString);
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
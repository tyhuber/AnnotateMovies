using System;
using HtmlAgilityPack;
using IpaExtensions.Objects;

namespace AnnotateMovieDirectories.Movies.RogerEbert
{
    public static class EbertQuery
    {
        private const string BaseUrl = "http://www.rogerebert.com/reviews/";
        public static bool TryGetRating(string title, string year, out int rating)
        {
            rating = -1;
            string ratingString;
            if (!Query(title, year, out ratingString))
                return false;
            double dbl;
            if (!double.TryParse(ratingString, out dbl))
            {
                return false;
            }
            rating = Convert.ToInt32(dbl);
            return true;
        }
        public static int GetRating(string title, string year)
        {
            string ratingString;
            if (!Query(title, year, out ratingString))
                return 0;
            double dbl;
            if (!double.TryParse(ratingString, out dbl))
            {
                return 0;
            }
            return Convert.ToInt32(dbl);
        }

        public static bool Query(string title, string year, out string ratingString)
        {
            title = title.ToLowerInvariant().Replace(" ", "-");
            string url =
                $"{BaseUrl}{title}-{year}";
            ratingString = string.Empty;
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            var exception = doc.DocumentNode.SelectSingleNode("//body[@id=\"exception\"]");
            if (exception != null)
            {
                return false;
            }
            var node = doc.DocumentNode.SelectSingleNode("//meta[@itemprop=\"ratingValue\"]");
            if (node == null)
                return false;
                

            if (node.HasAttributes)
            {
                var attributes = node.Attributes;
                if (attributes.Contains("content"))
                {
                    ratingString = attributes["content"].Value;
                }
            }
            return !string.IsNullOrWhiteSpace(ratingString);
        }

    }
}
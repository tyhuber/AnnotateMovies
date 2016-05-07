using System;
using System.Runtime.CompilerServices;
using AnnotateMovieDirectories.Logging;
using RestSharp;
using RestSharp.Deserializers;

namespace AnnotateMovieDirectories.Movies.Omb
{
    public static class OmdbSearch
    {
        public static OmdbResult Query(string name, string year, bool fullPlot = true)
        {
            name = name.Trim().Replace(' ', '+');
            if (name.Contains("&#x27;")) name = name.Replace("&#x27;", "'");
            
            var omdb = PrivateQuery(name, year);
            if (omdb!=null)
            {
                var shortOmdb = PrivateQuery(name, year, false);
                if (!omdb.Plot.Equals(shortOmdb.Plot))
                {
                    omdb.ShortPlot = shortOmdb.Plot;
                }
            }
            return omdb;
        }

        private static OmdbResult PrivateQuery(string name, string year, bool fullPlot = true)
        {
            string plotString = fullPlot ? "full" : "short";
            string url = $"http://www.omdbapi.com/?t={name}&y={year}&plot={plotString}&r=json&tomatoes=true";
            RestClient client = new RestClient();
            client.BaseUrl = new Uri(url);
            RestRequest req = new RestRequest();

            req.Method = Method.GET;

            var response = client.Execute<OmdbResult>(req);
            try
            {
                JsonDeserializer deserializer = new JsonDeserializer();
                OmdbResult mov = deserializer.Deserialize<OmdbResult>(response);
                Log(mov.ToString());
                return mov;
            }
            catch (Exception e)
            {
                Error(e);
                return default(OmdbResult);
            }
        }

        public static OmdbResult Query(string name)
        {
            name = name.Replace(' ', '+');
            if (name.Contains("&#x27;")) name = name.Replace("&#x27;", "'");
            string url = $"http://www.omdbapi.com/?t={name}&plot=short&r=json&tomatoes=true";
            RestClient client = new RestClient();
            client.BaseUrl = new Uri(url);
            RestRequest req = new RestRequest();

            req.Method = Method.GET;

            var response = client.Execute<OmdbResult>(req);
            try
            {
                JsonDeserializer deserializer = new JsonDeserializer();
                OmdbResult mov = deserializer.Deserialize<OmdbResult>(response);
                Log(mov.ToString());
                return mov;
            }
            catch (Exception e)
            {
                Error(e);
                return new OmdbResult();
            }
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
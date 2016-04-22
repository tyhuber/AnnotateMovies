﻿using System;
using System.Runtime.CompilerServices;
using AnnotateMovieDirectories.Logging;
using RestSharp;
using RestSharp.Deserializers;

namespace AnnotateMovieDirectories.Omdb
{
    public static class OmdbSearch
    {
        public static OmdbResult Query(string name, string year)
        {
            name = name.Replace(' ', '+');
            if (name.Contains("&#x27;")) name = name.Replace("&#x27;", "'");
            string url = $"http://www.omdbapi.com/?t={name}&y={year}&plot=full&r=json&tomatoes=true";
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
namespace AnnotateMovieDirectories.Extensions.Quality
{
    public static class Rip
    {
        public static RipType ParseType(string ripType)
        {
            //BluRay|BRRip|BDrip|DVDRip|DVDSCR|WEBRip
            switch (ripType.ToLowerInvariant())
            {
                case "bluray":
                    return RipType.BluRay;
                case "brrip":
                    return RipType.BluRay;
                case "bdrip":
                    return RipType.BluRay;
                case "dvdrip":
                    return RipType.DVD;
                case "dvdscr":
                    return RipType.DVD;
                case "webrip":
                    return RipType.Web;
                case "hdrip":
                    return RipType.HDRip;
                case "cam":
                    return RipType.Cam;
                default:
                    return RipType.Unknown;
            }
        }
    }
}
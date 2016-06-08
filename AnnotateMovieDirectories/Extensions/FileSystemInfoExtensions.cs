using System.IO;

namespace AnnotateMovieDirectories.Extensions
{
    public static class FileSystemInfoExtensions
    {
        public static bool ExistsNow(this FileSystemInfo fs)
        {
            fs.Refresh();
            return fs.Exists;
        }
    }
}
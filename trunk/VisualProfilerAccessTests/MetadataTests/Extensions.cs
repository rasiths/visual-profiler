using System.IO;

namespace VisualProfilerAccessTests.MetadataTests
{
    static class Extensions
    {
        public static MemoryStream ConvertToMemoryStream(this byte[] byteArray)
        {
            MemoryStream memoryStream = new MemoryStream(byteArray);
            return memoryStream;
        }
    }
}
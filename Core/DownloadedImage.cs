using System;

namespace Core
{
    public class DownloadedImage
    {
        public string Name { get; set; }
        public Uri Link { get; set; }
        public string FileFormat { get; set; }
        public byte[] Content { get; set; }
    }
}
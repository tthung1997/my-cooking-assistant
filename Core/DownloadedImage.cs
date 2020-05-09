using Newtonsoft.Json;
using System;

namespace Core
{
    public class DownloadedImage
    {
        public Guid Id { get; set; }
        public Uri Link { get; set; }
        public string FileFormat { get; set; }
        public byte[] Content { get; set; }

        public bool ShouldSerializeContent() => false;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
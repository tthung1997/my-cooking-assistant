using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public interface IImageClient
    {
        Task<IEnumerable<DownloadedImage>> GetImages(string query, int count);
    }
}

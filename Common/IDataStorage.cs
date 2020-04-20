using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface IDataStorage
    {
        Task<byte[]> DownloadFile(string fileName);
        Task<bool> UploadFile(string fileName, byte[] content);
        Task<bool> DeleteFile(string fileName);
        Task<bool> FileExists(string fileName);
    }
}
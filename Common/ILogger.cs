using System;
using System.IO;
using System.Threading.Tasks;

namespace Common
{
    public interface ILogger
    {
        Task LogInfo(string source, string message);
        Task LogError(string source, string message);
        Task LogError(string source, Exception exception);
        Task LogWarning(string source, string message);
    }

    public class ConsoleLogger : ILogger
    {
        public async Task LogError(string source, string message)
        {
            await Task.Run(() => PrintConsole($"[ERROR][{source}] {message}"));
        }

        public async Task LogError(string source, Exception exception)
        {
            await LogError(source, $"{exception.GetBaseException().Message}. " +
                $"Trace: {exception.GetBaseException().StackTrace}");
        }

        public async Task LogInfo(string source, string message)
        {
            await Task.Run(() => PrintConsole($"[INFO][{source}] {message}"));
        }

        public async Task LogWarning(string source, string message)
        {
            await Task.Run(() => PrintConsole($"[WARNING][{source}] {message}"));
        }

        private void PrintConsole(string message)
        {
            Console.WriteLine(message);
        }
    }

    public class FileLogger : ILogger
    {
        private readonly string filePath;

        public FileLogger(string filePath)
        {
            var fullPath = Path.GetFullPath(filePath);
            var directory = Path.GetDirectoryName(fullPath);
            var fileName = Path.GetFileName(fullPath);
            
            // create directory and subdirectory if they do not exist
            Directory.CreateDirectory(directory);

            // verify file name
            if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new ArgumentException("Invalid file name");
            }

            this.filePath = filePath;
        }

        public async Task LogError(string source, string message)
        {
            await Task.Run(() => WriteToFile($"[ERROR][{source}] {message}"));
        }

        public async Task LogError(string source, Exception exception)
        {
            await LogError(source, $"{exception.GetBaseException().Message}. " +
                $"Trace: {exception.GetBaseException().StackTrace}");
        }

        public async Task LogInfo(string source, string message)
        {
            await Task.Run(() => WriteToFile($"[INFO][{source}] {message}"));
        }

        public async Task LogWarning(string source, string message)
        {
            await Task.Run(() => WriteToFile($"[WARNING][{source}] {message}"));
        }

        private void WriteToFile(string message)
        {
            File.WriteAllText(filePath, message);
        }
    }
}

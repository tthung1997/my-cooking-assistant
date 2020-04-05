using System;
using System.IO;
using System.Threading.Tasks;

namespace Common
{
    public interface ILogger
    {
        Task LogInfo(string message);
        Task LogError(string message);
        Task LogError(Exception exception);
        Task LogWarning(string message);
    }

    public class ConsoleLogger : ILogger
    {
        public async Task LogError(string message)
        {
            await Task.Run(() => PrintConsole($"[ERROR] {message}"));
        }

        public async Task LogError(Exception exception)
        {
            await LogError($"{exception.GetBaseException().Message}. " +
                $"Trace: {exception.GetBaseException().StackTrace}");
        }

        public async Task LogInfo(string message)
        {
            await Task.Run(() => PrintConsole($"[INFO] {message}"));
        }

        public async Task LogWarning(string message)
        {
            await Task.Run(() => PrintConsole($"[WARNING] {message}"));
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
            var directory = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileName(filePath);
            
            // create directory and subdirectory if they do not exist
            Directory.CreateDirectory(directory);

            // verify file name
            if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new ArgumentException("Invalid file name");
            }

            this.filePath = filePath;
        }

        public async Task LogError(string message)
        {
            await Task.Run(() => WriteToFile($"[ERROR] {message}"));
        }

        public async Task LogError(Exception exception)
        {
            await LogError($"{exception.GetBaseException().Message}. " +
                $"Trace: {exception.GetBaseException().StackTrace}");
        }

        public async Task LogInfo(string message)
        {
            await Task.Run(() => WriteToFile($"[INFO] {message}"));
        }

        public async Task LogWarning(string message)
        {
            await Task.Run(() => WriteToFile($"[WARNING] {message}"));
        }

        private void WriteToFile(string message)
        {
            File.WriteAllText(filePath, message);
        }
    }
}

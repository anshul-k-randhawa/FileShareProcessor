using Azure.Storage.Files.Shares;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FileShareProcessor.Helpers
{
    public class FileHelper
    {
        private readonly string _connectionString;
        private readonly string _shareName;
        private readonly string _destFilePath;
        private readonly string _pattern;
        private readonly ShareFileClient _file;
        public FileHelper(ShareFileClient client, string connectionString, string shareName, string destFilePath, string pattern)
        {
            _file = client;
            _connectionString = connectionString;
            _shareName = shareName;
            _destFilePath = destFilePath;
            _pattern = pattern;
        }

        public async Task ProcessFile()
        {
            try
            {
                string line;
                bool isMatch = false;
                //read file line by line
                var reader = new StreamReader(await _file.OpenReadAsync());                
                while ((line = reader.ReadLine()) != null && !isMatch)
                {
                    isMatch = line.IsPatternMatch(_pattern);
                }
                reader.Close();
                if (isMatch)
                    await CopyFileAsync(_file, _connectionString, _shareName, _destFilePath);

                await DeleteFileAsync(_file);
            }
            catch (Exception e)
            {
                throw new Exception($"Error while processing file {_file.Name}. Error: {e.Message}. Stack trace: {e.StackTrace}");
            }
        }

        private async Task DeleteFileAsync(ShareFileClient file)
        {            
            await file.DeleteIfExistsAsync();
            Console.WriteLine($"{file.Uri} deleted.");
        }

        private async Task CopyFileAsync(ShareFileClient file, string connectionString, string shareName, string destFilePath)
        {
            // Get a reference to the destination file
            var destFile = new ShareFileClient(connectionString, shareName, string.Format("{0}/{1}", destFilePath, file.Name));

            // Start the copy operation
            await destFile.StartCopyAsync(file.Uri);
            if (await destFile.ExistsAsync())
            {
                Console.WriteLine($"{file.Uri} copied to {destFile.Uri}");
            }
        }
    }
}

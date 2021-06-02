using Azure.Storage.Files.Shares;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FileShareProcessor.Helpers
{
    public static class FileHelper
    {
        public static async Task ProcessFile(ShareFileClient file, string connectionString, string shareName, string destFilePath, string pattern)
        {
            try
            {
                string line;
                bool isMatch = false;
                //read file line by line
                var reader = new StreamReader(await file.OpenReadAsync());                
                while ((line = reader.ReadLine()) != null && !isMatch)
                {
                    isMatch = line.IsPatternMatch(pattern);
                }
                reader.Close();
                if (isMatch)
                    await CopyFileAsync(file, connectionString, shareName, destFilePath);

                await DeleteFileAsync(file);
            }
            catch (Exception e)
            {
                throw new Exception($"Error while processing file {file.Name}. Error: {e.Message}. Stack trace: {e.StackTrace}");
            }
        }

        private static async Task DeleteFileAsync(ShareFileClient file)
        {            
            await file.DeleteIfExistsAsync();
            Console.WriteLine($"{file.Uri} deleted.");
        }

        private static async Task CopyFileAsync(ShareFileClient file, string connectionString, string shareName, string destFilePath)
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

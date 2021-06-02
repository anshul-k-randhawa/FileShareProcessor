using Azure.Storage.Files.Shares;
using FileShareProcessor.Helpers;
using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Threading.Tasks;

namespace FileShareProcessor
{
    public static class Processor
    {        
        public static async Task ProcessDirectoryAsync(string connectionString, string shareName, string directoryName, string destFilePath, string pattern)
        {
            try
            {
                var parallellism = ConfigurationManager.AppSettings["Parallelism"];
                var exceptions = new ConcurrentQueue<Exception>();
                // Instantiate a ShareClient which will be used to create and manipulate the file share
                ShareClient share = new ShareClient(connectionString, shareName);

                // Ensure that the share exists
                if (await share.ExistsAsync())
                {
                    // Get a reference to the sample directory
                    ShareDirectoryClient directory = share.GetDirectoryClient(directoryName);

                    // Ensure that the directory exists
                    if (await directory.ExistsAsync())
                    {
                        var contents = directory.GetFilesAndDirectories();
                        //process files in parallel
                        Parallel.ForEach(contents, new ParallelOptions { MaxDegreeOfParallelism = string.IsNullOrEmpty(parallellism) ? 15: Convert.ToInt32(parallellism) }, item  =>
                        {
                            try
                            {
                                //skip directory
                                if (item.IsDirectory)
                                {
                                    Console.WriteLine($"Skipping directory item {item.Name}.");
                                }
                                else if(!item.Name.EndsWith(".txt"))
                                {
                                    Console.WriteLine($"Skipping file {item.Name} as it is not txt file.");
                                }
                                else
                                {
                                    // Get a reference to a file object
                                    ShareFileClient file = directory.GetFileClient(item.Name);

                                    if (file.Exists())
                                    {  
                                        FileHelper.ProcessFile(file, connectionString, shareName, destFilePath, pattern).Wait();
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                exceptions.Enqueue(e);
                            }
                        });
                        if (exceptions.Count > 0) throw new AggregateException(exceptions);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        
    }
}

using Azure.Storage.Files.Shares;
using FileShareProcessor.Helpers;
using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Threading.Tasks;

namespace FileShareProcessor
{
    public class Processor
    {
        private readonly string _connectionString;
        private readonly string _shareName;
        private readonly string _directoryName;
        private readonly string _destFilePath;
        private readonly string _pattern;

        public Processor(string connectionString, string shareName, string directoryName, string destFilePath, string pattern)
        {
            _connectionString = connectionString;
            _shareName = shareName;
            _directoryName = directoryName;
            _destFilePath = destFilePath;
            _pattern = pattern;
        }
        public async Task ProcessDirectoryAsync()
        {
            try
            {
                var parallellism = ConfigurationManager.AppSettings["Parallelism"];
                var exceptions = new ConcurrentQueue<Exception>();
                // Instantiate a ShareClient which will be used to create and manipulate the file share
                ShareClient share = new ShareClient(_connectionString, _shareName);

                // Ensure that the share exists
                if (await share.ExistsAsync())
                {
                    // Get a reference to the sample directory
                    ShareDirectoryClient directory = share.GetDirectoryClient(_directoryName);

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
                                        var fileHelper = new FileHelper(file, _connectionString, _shareName, _destFilePath, _pattern);
                                        fileHelper.ProcessFile().Wait();
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

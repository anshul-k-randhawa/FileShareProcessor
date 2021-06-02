using System;
using System.Configuration;
using FileShareProcessor;

namespace FileShareConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
            var shareName = ConfigurationManager.AppSettings["ShareName"];
            var directoryName = ConfigurationManager.AppSettings["DirectoryName"];
            var pattern = ConfigurationManager.AppSettings["Pattern"];
            var destFilePath = ConfigurationManager.AppSettings["DestinationFilePath"];
            try
            {
                var result = Processor.ProcessDirectoryAsync(connectionString, shareName, directoryName, destFilePath, pattern);
                //do some other task
                result.Wait();
            }
            catch (AggregateException ae)
            {
                Console.WriteLine($"Error occurred while processing dir {directoryName}.");
                foreach (var ex in ae.Flatten().InnerExceptions)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while processing dir {directoryName}. Error detail: {ex.Message}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
    }
}

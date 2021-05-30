# FileShareProcessor
Process files in file share

This is a console app which runs on a specific file share folder. Console app takes input from configuration file.

App will run a pattern (config key="Pattern") for each text line of the file in the folder (config key="DirectoryName") and if any matched then the file needs to be moved into a separate output folder (config key="DestinationFilePath") or else is deleted from its original location.
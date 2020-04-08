namespace ComicListener
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FileTypeDetective;
    using System.Threading;
    using System.IO;
    using System.Text.RegularExpressions;
    using ComicListener.Models;
    using ComicListener.Models.DTO;
    public class PathProcessor
    {
        public Dictionary<int, Thread> converterThreads = new Dictionary<int, Thread>();

        public static int LimitOfConversionAtOnce { get; set; }
        public string PathToDir { get; set; }

        public PathProcessor(string pathToDir)
        {
            this.PathToDir = pathToDir;
            PathProcessor.LimitOfConversionAtOnce = 4;
        }

        public void ProcessPath(string? targetDirectory)
        {
            targetDirectory = String.IsNullOrEmpty(targetDirectory) ? this.PathToDir : targetDirectory;

            if (File.Exists(targetDirectory))
            {
                this.ProcessFile(this.PathToDir);
            }
            else if (!Directory.Exists(this.PathToDir))
            {
                Console.WriteLine("{0} is not a valid file or directory.", this.PathToDir);
            }

            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
            {
                this.ProcessFile(fileName);
            }

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
            {
                this.ProcessPath(subdirectory);
            }
        }

        public void ProcessFile(string path)
        {
            // move all properties from obj file to ComicFile class
            FileInfo file = new FileInfo(path);

            string tempDir = file.Name.Replace(file.Extension, string.Empty);
            string dirName = string.Join(string.Empty, tempDir);

            // omit certain folders and file's suffix
            // convert into .ini file with chosen phrases to filter
            List<string> BannedExts = new List<string>(){
                ".part",
                ".ini"
            };
            List<string> BannedWordsInPath = new List<string>()
            {
                "tmp",
                "ads"
            };

            if (FilterFile(new FileInfoDTO(file), BannedExts, BannedWordsInPath))
            {
                return;
            }

            IComicFile fileToProcess = ComicsFileFactory.CreateNewComicObject(file);

            runConversionOfFile(fileToProcess);

        }

        public bool FilterFile(FileInfoDTO file, List<string> BannedExts, List<string> BannedWordsInPath)
        {
            return CheckStrByList(file.Extension, BannedExts) || CheckStrByList(file.FullPath, BannedWordsInPath);
        }


        public bool CheckStrByList(string stringToTest, List<string> bannedWords)
        {
            bool stringContains = false;
            bannedWords.ForEach(bannedWord =>
            {
                if (stringToTest.Contains(bannedWord))
                {
                    stringContains = true;
                }
            });

            return stringContains;
        }

        public void runConversionOfFile(IComicFile fileToProcess)
        {
            Thread convertionThread = new Thread(() => fileToProcess.ConvertSelfToZip());
            convertionThread.Name = fileToProcess.FileName;
            convertionThread.Priority = ThreadPriority.Highest;
            // run few convertions simultaneously 
            // could be done more elegant
            if (this.converterThreads.Count() >= PathProcessor.LimitOfConversionAtOnce)
            {
                while (true)
                {
                    foreach (KeyValuePair<int, Thread> thread in this.converterThreads)
                    {
                        if (this.converterThreads[thread.Key].ThreadState.ToString() == "Stopped")
                        {
                            try
                            {
                                this.converterThreads.Remove(thread.Key);
                                this.AddThreadToStack(thread.Key, convertionThread);
                                return;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                                Console.WriteLine(e.ToString());
                            }
                        }
                    }
                }
            }
            else
            {
                this.AddThreadToStack(this.converterThreads.Count, convertionThread);
            }
        }

        public void AddThreadToStack(int threadId, Thread threadToAdd)
        {
            this.converterThreads.Add(threadId, threadToAdd);
            this.converterThreads[threadId].Start();
        }
    }


}

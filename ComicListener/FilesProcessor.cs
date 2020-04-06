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
        private Dictionary<int, Thread> converterThreads = new Dictionary<int, Thread>();

        public List<string> FilesToGroup { get; set; }

        public static int LimitOfConversionAtOnce { get; set; }
        public string PathToDir { get; set; }

        public PathProcessor(string pathToDir)
        {
            this.PathToDir = pathToDir;
            this.FilesToGroup = new List<string>();
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

            IComicFile fileToProcess;

            // determine archieve type
            if (file.IsZip())
            {
                fileToProcess = new CbzComicFile(file);
            }
            else if (file.IsRar())
            {
                fileToProcess = new CbrComicFile(file);
            }
            else if (file.IsPdf())
            {
                fileToProcess = new PdfComicFile(file);
            }
            else
            {
                return;
            }

            // run few convertions simultaneously 
            if (this.converterThreads.Count() >= PathProcessor.LimitOfConversionAtOnce)
            {
                while (true)
                {
                    for (var i = 0; i < this.converterThreads.Count(); i++)
                    {
                        if (this.converterThreads[i].ThreadState.ToString() == "Stopped")
                        {
                            try
                            {
                                this.converterThreads.Remove(i);
                                Thread one = new Thread(() => fileToProcess.ConvertSelfToZip());
                                one.Name = file.Name;
                                one.Priority = ThreadPriority.Highest;
                                this.converterThreads.Add(i, one);
                                this.converterThreads[i].Start();
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
                Thread one = new Thread(() => fileToProcess.ConvertSelfToZip());
                one.Name = file.Name;
                this.converterThreads.Add(int.Parse(this.converterThreads.LongCount().ToString()), one);
                this.converterThreads.Last().Value.Start();
            }
        }

        // checks if all convertion processes ended
        public bool IfEndedAll()
        {
            var value = true;
            foreach (var i in Enumerable.Range(0, this.converterThreads.Count() - 1))
            {
                if (this.converterThreads[i].ThreadState.ToString() == "Stopped")
                {
                    value &= true;
                }
                else
                {
                    value &= false;
                }
            }

            return value;
        }

        public bool FilterFile(FileInfoDTO file, List<string> BannedExts, List<string> BannedWordsInPath)
        {
            foreach (var bannedExt in BannedExts)
            {

                if (file.Extension == bannedExt)
                {
                    return true;
                }
            }

            foreach (var bannedWord in BannedWordsInPath)
            {
                if (file.FullPath.Contains(bannedWord))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

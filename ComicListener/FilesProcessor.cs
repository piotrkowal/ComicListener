namespace ComicListener
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FileTypeDetective;
    using System.Threading;
    using System.IO;
    using System.Text.RegularExpressions;

    public class FilesProcessor
    {
        private Dictionary<int, Thread> converterThreads = new Dictionary<int, Thread>();

        public List<string> FilesToGroup { get; set; }

        public Dictionary<string, string> GroupOfFiles { get; set; }

        public List<Container> Groups { get; private set; }

        public static int LimitOfConversionAtOnce { get; set; }

        public string PathToDir { get; set; }

        public FilesProcessor(string pathToDir)
        {
            this.PathToDir = pathToDir;
            this.FilesToGroup = new List<string>();
            this.Groups = new List<Container>();
            FilesProcessor.LimitOfConversionAtOnce = 4;
        }

        public List<string> ProcessSelf()
        {
            if (Directory.Exists(this.PathToDir))
            {
                // This path is a directory
                this.ProcessDirectory(this.PathToDir);
            }
            else if (File.Exists(this.PathToDir))
            {
                // This path is a file
                this.ProcessFile(this.PathToDir);
            }
            else
            {
                Console.WriteLine("{0} is not a valid file or directory.", this.PathToDir);
            }

            return this.FilesToGroup;
        }

        public void ProcessDirectory(string targetDirectory)
        {
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
                this.ProcessDirectory(subdirectory);
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
            if (file.Extension == ".part" || file.Extension == ".ini" || file.Directory.ToString().Contains("tmp") || file.Directory.ToString().Contains("ads"))
            {
                return;
            }

            ComicFile fileToProcess = new CbzComicFile(file);

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

            // check if file could be grouped
            if (file.IsPdf() || file.IsRar() || file.IsZip())
            {
                this.FilesToGroup.Add(fileToProcess.ExtractDirectoryName + ".cbz");
            }

            // run few convertions simultaneously 
            if (this.converterThreads.Count() >= FilesProcessor.LimitOfConversionAtOnce)
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
                                Thread one = new Thread(() => fileToProcess.ProcessSelf());
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
                Thread one = new Thread(() => fileToProcess.ProcessSelf());
                one.Name = file.Name;
                this.converterThreads.Add(int.Parse(this.converterThreads.LongCount().ToString()), one);
                this.converterThreads.Last().Value.Start();
            }
        }

        // checks if all convertion processes ended
        public bool IfEndedAll()
        {
            var value = true;
            for (var i = 0; i < this.converterThreads.Count(); i++)
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
    }
}

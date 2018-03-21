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

        // to refactor, looks very messy
        public void GroupArchieves()
        {
            List<int> excluded = new List<int>();
            for (int j = 0; j < this.FilesToGroup.Count(); j++)
            {
                for (int i = j + 1; i < this.FilesToGroup.Count(); i++)
                {
                    if (i != j)
                    {
                        var firstString = this.FilesToGroup[j].Substring(0, this.FilesToGroup[j].LastIndexOf("."));
                        var secondString = this.FilesToGroup[i].Substring(0, this.FilesToGroup[i].LastIndexOf("."));
                        double similarity = SimilarityCalculator.CalculateSimilarity(this.FilesToGroup[i], this.FilesToGroup[j]);
                        Console.WriteLine("{0} {1}", firstString, secondString);
                        Console.WriteLine(similarity);
                        Regex regex = new Regex(@"^\w+?(.|,|!)?[^(][^\d$]+");
                        if (!firstString.Contains("("))
                        {
                            regex = new Regex(@"^(\w+ +\w+){1,200}[^\d$]");
                        }

                        Match match = regex.Match(firstString);
                        Regex regex2 = new Regex(@"^\w+?(.|,|!)?[^(][^\d$]+");
                        if (!secondString.Contains("("))
                        {
                            regex2 = new Regex(@"^(\w+ +\w+){1,200}[^\d$]");
                        }

                        Match match2 = regex.Match(secondString);
                        similarity = SimilarityCalculator.CalculateSimilarity(match.Value, match2.Value);

                        if (this.Groups.Where(x => x.FileName == firstString).Count() == 0)
                        {
                            Container contOriginal = new Container(firstString);

                            if (!match.Success)
                            {
                                regex = new Regex(@"^\w+?(.|,|!)?[^(][^\d$]+");
                            }

                            match = regex.Match(firstString);
                            contOriginal.Group = match.Value.Trim();
                            Console.WriteLine("dodajemy original {0} - {1}", contOriginal.FileName, contOriginal.Group);
                            this.Groups.Add(contOriginal);
                        }

                        if (similarity > 0.50)
                        {
                            Console.WriteLine("{0} {1}", match.Value, match2.Value);
                            Console.WriteLine(similarity);
                            Console.WriteLine("Connection: {0}        " + match.Value, secondString);
                            excluded.Add(i);
                            Container cont = new Container(secondString);

                            cont.Group = match.Value;
                            if (this.Groups.Where(x => x.FileName == cont.FileName).Count() == 0)
                            {
                                this.Groups.Add(cont);
                            }
                        }
                    }
                }
            }

            for (int j = 0; j < this.FilesToGroup.Count(); j++)
            {
                if (!excluded.Contains(j))
                {
                    var firstString = this.FilesToGroup[j].Substring(0, this.FilesToGroup[j].LastIndexOf("."));
                    Regex regex = new Regex(@"^\w+?(.|,|!)?[^(]+");
                    if (!firstString.Contains("("))
                    {
                        regex = new Regex(@"^(\w+ +\w+){1,200}");
                    }

                    Match match = regex.Match(firstString);
                    if (this.Groups.Where(x => x.FileName == firstString).Count() == 0)
                    {
                        Container contOriginal = new Container(firstString);

                        contOriginal.Group = match.Value;
                        Console.WriteLine("dodajemy original {0} - {1}", contOriginal.FileName, contOriginal.Group);
                        this.Groups.Add(contOriginal);
                    }
                }
            }

            for (int i = 0; i < excluded.Count(); i++)
            {
                Console.WriteLine(this.FilesToGroup[excluded[i]]);
            }

            foreach (var file in this.Groups)
            {
                Console.WriteLine(file.FileName + " - " + file.Group);
            }
        }

        // Process all files in the directory passed in, recurse on any directories 
        // that are found, and process the files they contain.
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

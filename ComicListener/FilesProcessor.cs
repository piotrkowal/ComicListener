using System;
using System.Collections.Generic;
using System.Linq;
using FileTypeDetective;
using System.Threading;
using System.IO;
using System.Collections;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Readers;
using SharpCompress.Writers;
using SharpCompress.Archives.Tar;
using System.Text.RegularExpressions;

namespace ComicListener
{
    class FilesProcessor
    {
        public Dictionary<int, Thread> ConverterThreads = new Dictionary<int, Thread>();
        public List<string> FilesToGroup { get; set; }
        public Dictionary<string, string> GroupOfFiles { get; set; }
        public List<ArchieveModel> ArchieveList { get; set; }
        public List<Container> Groups;
        public string PathToDir { get; set; }
        public FilesProcessor(string PathToDir)
        {
            this.PathToDir = PathToDir;
            this.FilesToGroup = new List<string>();
            this.Groups = new List<Container>();
        }

        public List<string> ProcessSelf()
        {
            if (Directory.Exists(PathToDir))
            {
                // This path is a directory
                ProcessDirectory(PathToDir);
            }
            else if (File.Exists(PathToDir))
            {
                // This path is a file
                ProcessFile(PathToDir);
            }
            else
            {
                Console.WriteLine("{0} is not a valid file or directory.", PathToDir);
            }
            return this.FilesToGroup;
        }


        // to refactor, looks very messy
        public void GroupArchieves()
        {
            List<int> excluded = new List<int>();
            for (int j = 0; j < FilesToGroup.Count(); j++)
            {
                for (int i = j + 1; i < FilesToGroup.Count(); i++)
                {
                    if (i != j)
                    {

                        var firstString = FilesToGroup[j].Substring(0, FilesToGroup[j].LastIndexOf("."));
                        var secondString = FilesToGroup[i].Substring(0, FilesToGroup[i].LastIndexOf("."));
                        double similarity = SimilarityCalculator.CalculateSimilarity(FilesToGroup[i], FilesToGroup[j]);
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

                        if (Groups.Where(x => x.fileName == firstString).Count() == 0)
                        {
                            Container contOriginal = new Container(firstString);

                            if (!match.Success)
                            {
                                regex = new Regex(@"^\w+?(.|,|!)?[^(][^\d$]+");
                            }
                            match = regex.Match(firstString);
                            contOriginal.group = match.Value.Trim();
                            Console.WriteLine("dodajemy original {0} - {1}", contOriginal.fileName, contOriginal.group);
                            Groups.Add(contOriginal);
                        }

                        if (similarity > 0.50)
                        {
                            Console.WriteLine("{0} {1}", match.Value, match2.Value);
                            Console.WriteLine(similarity);
                            Console.WriteLine("Connection: {0}        " + match.Value, secondString);
                            excluded.Add(i);
                            Container cont = new Container(secondString);

                            cont.group = match.Value;
                            if (Groups.Where(x => x.fileName == cont.fileName).Count() == 0)
                            {
                                Groups.Add(cont);
                            }


                        }
                    }

                }
            }


            for (int j = 0; j < FilesToGroup.Count(); j++)
            {
                if (!excluded.Contains(j))
                {
                    var firstString = FilesToGroup[j].Substring(0, FilesToGroup[j].LastIndexOf("."));
                    Regex regex = new Regex(@"^\w+?(.|,|!)?[^(]+");
                    if (!firstString.Contains("("))
                    {
                        regex = new Regex(@"^(\w+ +\w+){1,200}");
                    }
                    Match match = regex.Match(firstString);
                    if (Groups.Where(x => x.fileName == firstString).Count() == 0)
                    {
                        Container contOriginal = new Container(firstString);

                        contOriginal.group = match.Value;
                        Console.WriteLine("dodajemy original {0} - {1}", contOriginal.fileName, contOriginal.group);
                        Groups.Add(contOriginal);
                    }
                }
            }
            for (int i = 0; i < excluded.Count(); i++)
            {

                Console.WriteLine(FilesToGroup[excluded[i]]);
            }



            foreach (var file in Groups)
            {
                Console.WriteLine(file.fileName + " - " + file.group);
            }
        }



        // Process all files in the directory passed in, recurse on any directories 
        // that are found, and process the files they contain.
        public void ProcessDirectory(string targetDirectory)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                ProcessFile(fileName);

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory);
        }

        public void ProcessFile(string path)
        {
            // move all properties from obj file to ComicFile class
            FileInfo file = new FileInfo(path);
            

                //  get name to extract files from file (archieve)
                //string dirName = "";
                //string[] tempDir = file.Name.Split(".");

                //for (int i = 0; i < tempDir.Length - 1; i++)
                //{
                //    dirName += tempDir[i];
                //}
            string tempDir = file.Name.Replace(file.Extension,"");
            string dirName = string.Join("", tempDir);

            // omit certain folders and file's suffix
            if (file.Extension == ".part" || file.Directory.ToString().Contains("tmp") || file.Directory.ToString().Contains("ads"))
            {
                return;
            }

            ComicFile fileToProcess = new CbzComicFile(file) ;

            if (file.IsZip())
            {
                fileToProcess = new CbzComicFile(file);
            }

            if (file.IsRar())
            {
                fileToProcess = new CbrComicFile(file);
            }

            if (file.IsPdf())
            {
                fileToProcess = new PdfComicFile(file);
            }

            // check if file could be grouped
            if (file.IsPdf() || file.IsRar() || file.IsZip())
            {
                FilesToGroup.Add(fileToProcess.ExtractDirectoryName + ".cbz");

            }




            if (this.ConverterThreads.Count() >= 4)
            {
                while (true)
                {
                    for (var i = 0; i < ConverterThreads.Count(); i++)
                    {
                        if (ConverterThreads[i].ThreadState.ToString() == "Stopped")
                        {
                            try
                            {
                                ConverterThreads.Remove(i);
                                Thread one = new Thread(() => fileToProcess.ProcessSelf());
                                one.Name = file.Name;
                                one.Priority = ThreadPriority.Highest;
                                ConverterThreads.Add(i, one);
                                //Console.WriteLine(Program.ConverterThreads[i].ThreadState + " " + Program.ConverterThreads[i].Name);
                                ConverterThreads[i].Start();
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
                ConverterThreads.Add(int.Parse(ConverterThreads.LongCount().ToString()), one);
                ConverterThreads.Last().Value.Start();
            }


            //if (file.IsPdf())
            //{
            //    if (file.Extension != "pdf")
            //    {
            //        Console.WriteLine(@"Processed file has wrong extension.");

            //    }
            //    Console.WriteLine(@"Processed file $file.Name .");
            //}

            //if (file.IsRar())
            //{

            //    if (file.Extension != ".cbr")
            //    {
            //        Console.WriteLine($"Processed file has wrong extension.");
            //    }
            //    Console.WriteLine($"Processed file {file.Name}.");
 
            //    if (ConverterThreads.Count() >= 4)
            //    {   //wait and check if open

            //        while (true)
            //        {
            //            for (var i = 0; i < ConverterThreads.Count(); i++)
            //            {


            //                if (ConverterThreads[i].ThreadState.ToString() == "Stopped")
            //                {
            //                    ConverterThreads.Remove(i);
            //                    try
            //                    {
            //                        Thread one = new Thread(() => ArchivesConverter.ConvertToZip(file.Directory.ToString(), file.Name, dirName));
            //                        one.Name = file.Name;
            //                        one.Priority = ThreadPriority.Highest;
            //                        ConverterThreads.Add(i, one);
            //                        ConverterThreads[i].Start();
            //                        return;
            //                    }
            //                    catch (Exception e)
            //                    {

            //                        Console.WriteLine(e.Message);
            //                        Console.WriteLine(e.ToString());
            //                    }
                                
            //                }

            //            }
            //        }
            //    }
            //    else
            //    {
            //        Thread one = new Thread(() => ArchivesConverter.ConvertToZip(file.Directory.ToString(), file.Name, dirName));
            //        one.Name = file.Name;
            //        ConverterThreads.Add(int.Parse(ConverterThreads.LongCount().ToString()), one);
            //        ConverterThreads.Last().Value.Start();
            //    }
            //}

            //if (file.IsZip())
            //{

            //   if (file.Extension != ".cbz")
            //    {
            //        Console.WriteLine($"Processed file has wrong extension.");
            //    }
            //    Console.WriteLine($"Processed file {file.Name}.");

            //    if (ConverterThreads.Count() >= 4)
            //    {   //wait and check if open

            //        while (true)
            //        {
            //            for (var i = 0; i < ConverterThreads.Count(); i++)
            //            {


            //                if (ConverterThreads[i].ThreadState.ToString() == "Stopped")
            //                {


            //                    try
            //                    {
            //                        ConverterThreads.Remove(i);
            //                        Thread one = new Thread(() => ArchivesConverter.RecreateZip(file.Directory.ToString(), file.Name, dirName));
            //                        one.Name = file.Name;
            //                        one.Priority = ThreadPriority.Highest;
            //                        ConverterThreads.Add(i, one);
            //                        //Console.WriteLine(Program.ConverterThreads[i].ThreadState + " " + Program.ConverterThreads[i].Name);
            //                        ConverterThreads[i].Start();
            //                        return;
            //                    }
            //                    catch (Exception e)
            //                    {

            //                        Console.WriteLine(e.Message);
            //                        Console.WriteLine(e.ToString());
            //                    }
            //                }

            //            }
            //        }
            //    }
            //    else
            //    {
            //        Thread one = new Thread(() => ArchivesConverter.RecreateZip(file.Directory.ToString(), file.Name, dirName));
            //        one.Name = file.Name;
            //        ConverterThreads.Add(int.Parse(ConverterThreads.LongCount().ToString()), one);
            //        ConverterThreads.Last().Value.Start();
            //    }

            //}






        }

        public bool ifEndedAll()
        {
            var value = true;
            for (var i = 0; i < ConverterThreads.Count(); i++)
            {

                if (ConverterThreads[i].ThreadState.ToString() == "Stopped")
                {
                    value &= true;
                } else
                {
                    value &= false;
                }
            }
            return value;
        }
    }

    class Container
    {
        public string group { get; set; }
        public string fileName { get; set; }
        public string originalName { get; set; }
        public string originalSize { get; set; }
        public string originalFormat { get; set; }

        public Container(string name)
        {
            this.fileName = name;
        }
    }
}

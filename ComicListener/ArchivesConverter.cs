namespace ComicListener
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.IO.Compression;
    using SharpCompress.Archives;
    using SharpCompress.Archives.Rar;
    using SharpCompress.Readers;
    using SharpCompress.Common;
    using ComicListener.Models;

    public class ArchivesConverter
    {
        // Converts input file to Zip Archieve
        public static void ConvertArchieveToZip(ComicFile file)
        {
            // determine archieve format 
            try
            {
                if (file.Extension == ".cbz" && file.HasProperExtenstion)
                {
                    ExtractZipArchive(file);
                }
                else
                {
                    ExtractRarArchive(file);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.ToString());
            }

            // Check if exsists files that isn't similar to others
            string[] fileEntries = Directory.GetFiles(file.ExtractDirectoryPath);
            Dictionary<int, string> filesToMove = new Dictionary<int, string>();

            // take first file to compare to other files from folder
            string testFile;
            try
            {
                testFile = fileEntries[0];
            }
            catch
            {
                return;
            }

            for (var i = 1; i < fileEntries.Length; i++)
            {
                double similarity = SimilarityCalculator.CalculateSimilarity(testFile, fileEntries[i]);
                Console.WriteLine(similarity);
                if (similarity < 0.90)
                {
                    // check if it's variant cover -> TODO: do it in smarter way
                    if (fileEntries[i].Contains("variant") || fileEntries[i].Contains("Variant") || fileEntries[i].Contains("cover") || fileEntries[i].Contains("Cover"))
                    {
                        continue;
                    }

                    Directory.CreateDirectory(file.PathToDirectory + @"/ads/" + file.ExtractDirectoryName);

                    Console.WriteLine(fileEntries[i]);
                    filesToMove.Add(filesToMove.Count(), fileEntries[i]);
                }
            }

            // if first file isn't similar to others
            if (filesToMove.LongCount() > fileEntries.LongCount() / 2)
            {
                Console.WriteLine("Cośnie tak!");
                filesToMove.Clear();
                Regex regex = new Regex(@"^[0-9]+.jpg");
                Match match = regex.Match(testFile);
                if (!match.Success)
                {
                    filesToMove.Add(0, testFile);
                }
            }

            Console.WriteLine(filesToMove.LongCount());

            // move files to ./ads/ folder
            if (filesToMove.Count() != 0)
            {
                foreach (var fileToMove in filesToMove)
                {
                    FileInfo movedFile = new FileInfo(fileToMove.Value);
                    Console.WriteLine(movedFile.Name);
                    try
                    {
                        movedFile.MoveTo(file.PathToDirectory + @"/ads/" + file.ExtractDirectoryName + @"/" + file.FileName);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.ToString());
                    }
                }
            }

            // delete old file, create new zip file from extracted path
            TryToDelete(file.PathToDirectory + "//" + file.FileName);
            CreateZip(file.ExtractDirectoryPath, file.PathToDirectory + @"/" + file.ExtractDirectoryName + ".cbz");
            try
            {
                Directory.Delete(file.ExtractDirectoryPath, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.ToString());
                Console.Read();
            }
        }

        public static void CreateZip(string pathToZip, string pathToSave)
        {
            pathToZip = pathToZip.TrimEnd();
            ZipFile.CreateFromDirectory(pathToZip, pathToSave);
        }

        public static void ExtractRarArchive(ComicFile file)
        {
            Console.WriteLine(file.ExtractDirectoryPath);
            string pathToFile = file.PathToDirectory + "//" + file.FileName;

            var archieve2 = RarArchive.Open(pathToFile);
            using (var archive = archieve2)
            {
                Directory.CreateDirectory(file.ExtractDirectoryPath);
                Console.WriteLine($"Creating dir {file.ExtractDirectoryPath}");
                try
                {
                    foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                    {
                        entry.WriteToDirectory(
                            file.ExtractDirectoryPath,
                            new ExtractionOptions()
                            {
                                ExtractFullPath = false,
                                Overwrite = true
                            });
                        Console.WriteLine($"Extracting file {entry.Key}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
            }
        }

        public static void ExtractZipArchive(ComicFile file)
        {
            string pathToFile = file.PathToDirectory + "//" + file.FileName;

            try
            {
                var archieve2 = SharpCompress.Archives.Zip.ZipArchive.Open(pathToFile);
                using (var archive = archieve2)
                {
                    Directory.CreateDirectory(file.ExtractDirectoryPath);
                    Console.WriteLine($"Creating dir {file.ExtractDirectoryPath}");

                    foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                    {
                        entry.WriteToDirectory(
                            file.ExtractDirectoryPath.TrimEnd(),
                            new ExtractionOptions()
                            {
                                ExtractFullPath = false,
                                Overwrite = true
                            });
                        Console.WriteLine($"Extracting file {entry.Key}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void TryToDelete(string filePath)
        {
            while (true)
            {
                try
                {
                    File.Delete(filePath);
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}

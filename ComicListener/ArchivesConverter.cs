using System;
using System.Collections.Generic;
using System.IO;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Readers;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO.Compression;

namespace ComicListener
{
    class ArchivesConverter
    {


        public static void ConvertArchieveToZip(ComicFile file)
        {
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
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.ToString());
                Console.Read();
            }

            string[] fileEntries = Directory.GetFiles(file.ExtractDirectoryPath);
            Dictionary<int, string> filesToMove = new Dictionary<int, string>();
            string testFile;
            try
            {
                testFile = fileEntries[0];
            }
            catch (Exception e)
            {
                return;
            }

            for (var i = 1; i < fileEntries.Length; i++)
            {
                double similarity = SimilarityCalculator.CalculateSimilarity(testFile, fileEntries[i]);
                Console.WriteLine(similarity);
                if (similarity < 0.90)
                {
                    if (fileEntries[i].Contains("variant") || fileEntries[i].Contains("Variant") || fileEntries[i].Contains("cover") || fileEntries[i].Contains("Cover")) { continue; }
                    Directory.CreateDirectory(file.PathToDirectory + @"\ads\" + file.ExtractDirectoryName);

                    Console.WriteLine(fileEntries[i]);

                    filesToMove.Add(filesToMove.Count(), fileEntries[i]);

                }
            }

            Console.WriteLine(filesToMove.LongCount());
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

            if (filesToMove.Count() != 0)
            {
                foreach (var fileToMove in filesToMove)
                {
                    FileInfo movedFile = new FileInfo(fileToMove.Value);
                    Console.WriteLine(movedFile.Name);
                    try
                    {
                        movedFile.MoveTo(file.PathToDirectory + @"\ads\" + file.ExtractDirectoryName + @"\" + file.FileName);
                    }
                    catch (Exception e)
                    {

                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.ToString());
                    }
                }
            }

            File.Delete(file.PathToDirectory + "\\" + file.FileName);
            CreateZip(file.ExtractDirectoryPath, file.PathToDirectory+@"\"+ file.ExtractDirectoryName+".cbz");
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
        /*
        public static void ConvertToZip(string path, string fileName, string dirName)
        {
            string pathToFiles = path + "\\tmp\\" + dirName;
            string pathToZip = path + @"\" + dirName + ".cbz";
            TryToDelete(path + "\\" + fileName);
            CreateZip(pathToFiles, pathToZip, dirName);
            Directory.Delete(pathToFiles, true);
        }

        public static void RecreateZip(string path, string fileName, string dirName)
        {
           
            string pathToFiles = path + "\\tmp\\" + dirName;
            string pathToZip = path +@"\"+ dirName+".cbz";
            

        }
        */
        public static void CreateZip(string pathToZip, string pathToSave)
        {
            pathToZip = pathToZip.TrimEnd();

            ZipFile.CreateFromDirectory(pathToZip, pathToSave);

            //using (var archive = ZipArchive.Create())
            //{
            //    Console.WriteLine($"Creating zip");
            //    archive.AddAllFromDirectory(pathToZip);
            //    Console.WriteLine($"Saving zip");
            //    try
            //    {
            //        archive.SaveTo(pathToSave, CompressionType.LZMA);

            //    }
            //    catch
            //    {
            //        return;
            //    }
            //}
        }
        /*
        public static void ExtractArchieve(ComicFile file)
        {
            RarArchive rarArchieve = new RarArchive();
            ZipArchive zipArchive;
            Console.WriteLine(file.GetType().ToString());
            if (file.Extension == ".cbz")
            {
                zipArchive = ZipArchive.Open(file.PathToDirectory + "\\" + file.FileName);
            }
            else
            {
                rarArchieve = RarArchive.Open(file.ExtractDirectoryName);
            }

            var whichArchieve = rarArchieve. || zipArchive ? rarArchieve : zipArchive;
            using (var archive = archieve3)
            {
                Directory.CreateDirectory(file.ExtractDirectoryName);
                Console.WriteLine($"Creating dir {file.ExtractDirectoryName}");
                try
                {
                    if (file.Extension == ".cbz")
                    {
                        ZipArchive test = archive.Entries.Where(s => !s.IsDirectory);

                    }
                    else
                    {

                    }


                    foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                    {
                        //we don't want any folder inside new archieve
                        if (entry.IsDirectory)
                        {
                            continue;
                        }

                        entry.WriteToDirectory(file.ExtractDirectoryName.TrimEnd(), new ExtractionOptions()
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

        }*/
        public static void ExtractRarArchive(ComicFile file)
        {

            Console.WriteLine(file.ExtractDirectoryPath);
            string pathToFile = file.PathToDirectory + "\\" + file.FileName;

            var archieve2 = RarArchive.Open(pathToFile);
            using (var archive = archieve2)
            {
                Directory.CreateDirectory(file.ExtractDirectoryPath);
                Console.WriteLine($"Creating dir {file.ExtractDirectoryPath}");
                try
                {
                    foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                    {
                        entry.WriteToDirectory(file.ExtractDirectoryPath, new ExtractionOptions()
                        {
                            ExtractFullPath = false,
                            Overwrite = true
                        });
                        Console.WriteLine($"Extracting file {entry.Key}");
                    }
                } catch(Exception e) {
                    Console.WriteLine(e.Message);
                    Console.Beep();
                    Console.Read();
                    return;
                }
            }
        }

        public static void ExtractZipArchive(ComicFile file)
        {
            string pathToFile = file.PathToDirectory + "\\" + file.FileName;

            try
            {
                var archieve2 = SharpCompress.Archives.Zip.ZipArchive.Open(pathToFile);
                using (var archive = archieve2)
                {
                    Directory.CreateDirectory(file.ExtractDirectoryPath);
                    Console.WriteLine($"Creating dir {file.ExtractDirectoryPath}");

                    foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                    {

                        entry.WriteToDirectory(file.ExtractDirectoryPath.TrimEnd(), new ExtractionOptions()
                        {
                            ExtractFullPath = false,
                            Overwrite = true
                        });
                        Console.WriteLine($"Extracting file {entry.Key}");
                    }
                }
            } catch(Exception e)
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
                catch
                {

                }

            }
        }
    }
}

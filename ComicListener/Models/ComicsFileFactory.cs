using System.Collections.Generic;
using System.Linq;
using System.IO;
using FileTypeDetective;

namespace ComicListener.Models
{
    public class ComicsFileFactory
    {
        public static IComicFile CreateNewComicObject(FileInfo fileInfo)
        {
            IComicFile fileToProcess = null;
            Dictionary<string, bool> archieveFormat = new Dictionary<string, bool>();
            archieveFormat.Add("zip", fileInfo.IsZip());
            archieveFormat.Add("rar", fileInfo.IsRar());
            archieveFormat.Add("pdf", fileInfo.IsPdf());

            archieveFormat.AsParallel().ForAll(keyValuePair =>
            {
                if (keyValuePair.Value)
                {
                    switch (keyValuePair.Key)
                    {
                        case "zip":
                            fileToProcess = new CbzComicFile(fileInfo);
                            break;
                        case "rar":
                            fileToProcess = new CbrComicFile(fileInfo);
                            break;
                        case "pdf":
                            fileToProcess = new PdfComicFile(fileInfo);
                            break;
                        default:
                            fileToProcess = null;
                            break;
                    }
                }
            });

            return fileToProcess;
        }


    }
}
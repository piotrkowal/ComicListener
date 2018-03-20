using System;
using System.IO;
using System.Collections;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Readers;
using System.Linq;
using SharpCompress.Writers;
using FileTypeDetective;
using System.Threading;
using SharpCompress.Archives.Tar;
using System.Collections.Generic;

namespace ComicListener
{
    class Program
    {
        public static Dictionary<int,Thread> ConverterThreads = new Dictionary<int,Thread>();
        public static void Main(string[] args)
        {
           // string arg = @"G:\Conv";
            string arg = @"E:\test";
            FilesProcessor jarvis = new FilesProcessor(arg);
            List<string> wynik = jarvis.ProcessSelf();

            while (true)
            {
                if (jarvis.ifEndedAll())
                {
                    for (var i = 0; i < wynik.Count(); i++)
                    {
                        Console.WriteLine(wynik[i]);
                    }

                    jarvis.GroupArchieves();
                    break;
                }
            }

            Console.WriteLine("\n Results:");
            foreach (var item in jarvis.Groups)
                  Console.WriteLine(item.fileName+"   " + item.group);

            Console.Read();




        }
    }
}
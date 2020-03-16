namespace ComicListener
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Collections.Generic;

    public class Program
    {
        private static Dictionary<int, Thread> converterThreads = new Dictionary<int, Thread>();
        public static void Main(string[] args)
        {
            string filesPath;
            if (args.Length > 0)
            {
                filesPath = args[0];
            }
            else
            {
                filesPath = @"~/test";
            }

            FilesProcessor jarvis = new FilesProcessor(filesPath);
            List<string> wynik = jarvis.ProcessSelf();


            if (jarvis.IfEndedAll())
            {
                for (var i = 0; i < wynik.Count(); i++)
                {
                    Console.WriteLine(wynik[i]);
                }

                jarvis.GroupArchieves();
            }


            Console.WriteLine("\n Results:");
            foreach (var item in jarvis.Groups)
            {
                Console.WriteLine(item.FileName + "   " + item.Group);
            }

            Console.Read();
        }
    }
}
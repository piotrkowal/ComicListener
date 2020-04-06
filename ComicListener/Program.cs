namespace ComicListener
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Collections.Generic;
    using Serilog;

    public class Program
    {
        public static void Main(string[] args)
        {
            LoggerService.Log.Information("Hello, world!");

            string filesPath = args.Length > 0 ? args[0] : @"~/test";

            PathProcessor jarvis = new PathProcessor(filesPath);
            jarvis.ProcessPath(String.Empty);

        }
    }
}
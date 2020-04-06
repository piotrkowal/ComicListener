using Serilog;

namespace ComicListener
{
    public class LoggerService
    {
        public static ILogger Log = new LoggerConfiguration()
                .WriteTo.File("consoleapp.log")
                .CreateLogger();

    }
}
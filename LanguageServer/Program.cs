using System;
using System.IO;
using System.Text;

namespace CheezLanguageServer
{
    class Program
    {
        static void Main(string[] args)
        {


            Console.OutputEncoding = Encoding.UTF8;
            var app = new CheezLanguageServer(Console.OpenStandardInput(), Console.OpenStandardOutput());
            Logger.Instance.Attach(app);
            try
            {
                app.Listen().Wait();
            }
            catch (AggregateException ex)
            {
                Console.Error.WriteLine(ex.InnerExceptions[0]);
                Environment.Exit(-1);
            }
        }
    }
}

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CheezLanguageServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            //RunLanguageServerOverTcp(5007);
            RunLanguageServerOverStdInOut();
        }

        private static void LaunchLanguageServer(Stream inStream, Stream outStream)
        {
            try
            {
                var app = new CheezLanguageServer(inStream, outStream);
                Logger.Instance.Attach(app);
                app.Listen().Wait();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        private static void foo()
        {

        }

        private static void RunLanguageServerOverStdInOut()
        {
            using var _in = Console.OpenStandardInput();
            using var _out = Console.OpenStandardOutput();
            LaunchLanguageServer(_in, _out);
        }

        private static void RunLanguageServerOverTcp(int port)
        {
            Console.WriteLine("Running Language Server oper tcp");
            TcpListener server = null;
            try
            {
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                server = new TcpListener(localAddr, port);
                server.Start();

                while (true)
                {
                    Console.WriteLine("Waiting for client...");
                    using (var client = server.AcceptTcpClient())
                    using (var stream = client.GetStream())
                    {
                        Console.WriteLine($"Client connected from {client.Client.RemoteEndPoint}");
                        LaunchLanguageServer(stream, stream);
                        Console.WriteLine($"Client  disconnected");
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CheezLanguageServer
{
    public static class CheezLanguageServerLauncher
    {
        public static void RunLanguageServerOverStdInOut(long parentPid)
        {
            if (parentPid >= 0) {
                var killThread = new Thread(() => {
                    while (true) {
                        try {
                            var parentProcess = Process.GetProcessById((int)parentPid);
                        } catch (ArgumentException) {
                            // process no longer exists
                            System.Environment.Exit(1);
                        }

                        Thread.Sleep(2000);
                    }
                });
                killThread.Start();
            }
            // var currentProcess = Process.GetCurrentProcess();
            // if (Process.GetProcessById(123).)

            Console.OutputEncoding = Encoding.UTF8;
            using var _in = Console.OpenStandardInput();
            using var _out = Console.OpenStandardOutput();
            LaunchLanguageServer(_in, _out);
        }

        public static void RunLanguageServerOverTcp(int port)
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
    }
}

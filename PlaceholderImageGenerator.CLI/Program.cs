using System.Net.Sockets;
using System.Net;

namespace PlaceholderImageGenerator.CLI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            int port = 5000; 

            if (args.Length > 0 && int.TryParse(args[0], out int customPort) && customPort > 0 && customPort <= 65535)
            {
                port = customPort;
            }

            if (IsPortInUse(port))
            {
                Console.WriteLine($"Error: Port {port} is already in use. Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(1);
            }

            CreateHostBuilder(args, port).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, int port) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls($"http://localhost:{port}");
                });

        private static bool IsPortInUse(int port)
        {
            bool isInUse = false;
            try
            {
                var listener = new TcpListener(IPAddress.Loopback, port);
                listener.Start();
                listener.Stop();
            }
            catch (SocketException)
            {
                isInUse = true;
            }

            return isInUse;
        }
    }
}


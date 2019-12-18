using Renci.SshNet;
using Renci.SshNet.Common;
using System;

namespace SSHClientTurnel
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("SSH Client!");

            using (var client = new SshClient("52.231.201.187", "kevinoh", "Kevin!3245678"))
            {
                try
                {
                    client.Connect();
                }
                catch (Exception ex2)
                {
                    Console.WriteLine("SSH connection error!");
                    Console.WriteLine(ex2.ToString());
                }

                // SSH Remote Port Forwarding
                // 1234 : SSH server side port(remote port)
                // 554 : local service port (ex, 80, 3389 ...)
                // localhost: local service hostname or ipaddress
                var port = new ForwardedPortRemote(1234, "127.0.0.1", 8554);

                client.AddForwardedPort(port);

                port.Exception += delegate (object sender, ExceptionEventArgs e)
                {
                    Console.WriteLine(e.Exception.ToString());
                };
                port.RequestReceived += delegate (object sender, PortForwardEventArgs e)
                {
                    Console.WriteLine(e.OriginatorHost);
                };
                port.Start();

                // ... hold the port open ... //
                Console.WriteLine("Press any key to close.");
                Console.ReadLine();

                port.Stop();
                client.Disconnect();
            }
        }
    }
}

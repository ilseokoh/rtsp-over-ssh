using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ServerProxy
{
    class Program
    {
        const int BUFFER_SIZE = 4096;
        static void Main(string[] args)
        {
            var _localPort = 1234;
            var _remotePort = 554;

            Console.WriteLine($"Server TCP Proxy!");

            var listener = new TcpListener(IPAddress.Any, _remotePort);
            listener.Start();

            new Task(() => {
                // Accept clients.
                while (true)
                {
                    var client = listener.AcceptTcpClient();
                    Console.WriteLine($"Accept: {client.ReceiveBufferSize}");

                    new Task(() => {
                        // Handle this client.
                        var clientStream = client.GetStream();

                        // Start TCP server. This connects to SSH remote port fowarding port
                        TcpClient server = new TcpClient("127.0.0.1", _localPort);
                        var serverStream = server.GetStream();
                        new Task(() => {
                            byte[] message = new byte[BUFFER_SIZE];
                            int clientBytes;
                            while (true)
                            {
                                try
                                {
                                    clientBytes = clientStream.Read(message, 0, BUFFER_SIZE);
                                }
                                catch
                                {
                                    // Socket error - exit loop.  Client will have to reconnect.
                                    break;
                                }
                                if (clientBytes == 0)
                                {
                                    // Client disconnected.
                                    break;
                                }
                                serverStream.Write(message, 0, clientBytes);
                            }
                            client.Close();
                        }).Start();
                        new Task(() => {
                            byte[] message = new byte[BUFFER_SIZE];
                            int serverBytes;
                            while (true)
                            {
                                try
                                {
                                    serverBytes = serverStream.Read(message, 0, BUFFER_SIZE);
                                    clientStream.Write(message, 0, serverBytes);
                                }
                                catch
                                {
                                    // Server socket error - exit loop.  Client will have to reconnect.
                                    break;
                                }
                                if (serverBytes == 0)
                                {
                                    // server disconnected.
                                    break;
                                }
                            }
                        }).Start();
                    }).Start();
                    Thread.Sleep(10);
                }
            }).Start();

            Console.WriteLine($"Server listening on port {_remotePort}.  Press enter to exit.");
            Console.ReadLine();

            listener.Stop();
        }

    }
}

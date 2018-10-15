using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Transformice.Base
{
    public class StartServer
    {
        private ManualResetEvent allDone = new ManualResetEvent(false);

        private Server server = null;

        public StartServer(Server server)
        {
            this.server = server;
        }

        public void Listen(string address, int port)
        {
            IPEndPoint bindEndPoint = new IPEndPoint(IPAddress.Parse(address), port);
            Socket serverSocket = new Socket(bindEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                serverSocket.Bind(bindEndPoint);
                serverSocket.Listen(255);

                Console.WriteLine($"[Server Log] Running server on port {port}");

                while (true)
                {
                    allDone.Reset();
                    serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), serverSocket);
                    allDone.WaitOne();
                }

            }
            catch
            {
                // Do Nothing
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            allDone.Set();
            Socket serverSocket = (Socket)ar.AsyncState;
            Socket clientSocket = serverSocket.EndAccept(ar);

            Client client = new Client(this.server, clientSocket);
            client.Start();
        }
    }

    public class Server
    {
        public Server() { }
    }
}
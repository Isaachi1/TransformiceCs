using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Transformice.Base;

namespace Transformice
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();

            int[] ports = { 44440, 44444, 5555, 6112, 3724 };

            StartServer handler = new StartServer(server);

            foreach (int port in ports)
            {
                Thread thread = new Thread(() => handler.Listen("127.0.0.1", port));
                thread.Start();
            }
        }
    }
}
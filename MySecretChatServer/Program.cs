using System;
using System.Threading;

namespace MySecretChatServer
{
    class Program
    {
        static Server server;
        static Thread t;
        static void Main(string[] args)
        {
            //Console.WriteLine(MyTime.getTime());
            try
            {
                server = new Server();
                t = new Thread(new ThreadStart(server.Listen));
                t.Start();
            }
            catch
            {
                server.StopServer();
                Console.WriteLine(MyTime.getTime() + " Server is stopped");
            }
        }
    }
}

using System;
using System.Threading;
using TelepathyLink.Core;
using TelepathyLink.Core.Attributes;

namespace TelepathyLink.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new LinkServer();
            server.RegisterContracts();
            server.Start(8080);

            Console.ReadLine();
        }
    }
}

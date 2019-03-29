using System;
using System.Threading;
using TelepathyLink.Core;

namespace TelepathyLink.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new LinkServer();
            server.RegisterContracts();
            server.Start(8080);

            Thread.Sleep(1000);
            var client = new LinkClient();
            client.Setup("127.0.0.1", 8080);
            var contract = client.GetContract<ITestContract>();
            contract.StrikeBack.Subscribe(() => { Console.WriteLine("Kick back!"); });
            Console.ReadLine();
        }
    }
}

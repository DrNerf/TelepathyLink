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
            server.RegisterContractImplementation<ITest, TestImpl>(new TestImpl());
            server.Start(8080);

            var client = new LinkClient();
            Thread.Sleep(1000);
            client.Setup("127.0.0.1", 8080);
            var test = client.GetContract<ITest>();
            Console.WriteLine("Client: Test");
            Console.WriteLine(test.Test("Marco"));
            

            Console.ReadLine();
        }
    }

    [Contract]
    public interface ITest
    {
        string Test(string test);
    }

    public class TestImpl : ITest
    {
        public string Test(string test)
        {
           return test + " Pollo!";
        }
    }
}

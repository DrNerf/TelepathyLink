using System;
using TelepathyLink.Core;
using TelepathyLink.Core.Attributes;

namespace TelepathyLink.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new LinkClient();
            var contract = client.GetContract<ITest>();
            contract.Test();

            Console.ReadLine();
        }
    }

    [Contract]
    public interface ITest
    {
        void Test();
    }
}

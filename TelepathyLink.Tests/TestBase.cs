using NUnit.Framework;
using System.Threading;
using TelepathyLink.Core;

namespace TelepathyLink.Tests
{
    public abstract class TestBase
    {
        protected LinkServer Server { get; set; }
        protected LinkClient Client { get; set; }
        protected ITestContract TestContract { get; set; }
        protected TestContract TestContractInstance { get; set; }

        [SetUp]
        public void Setup()
        {
            Server = new LinkServer();
            TestContractInstance = new TestContract();
            Server.RegisterContractImplementation<ITestContract, TestContract>(TestContractInstance);
            Server.Start(8080);

            // Give the server a second to fire up.
            Thread.Sleep(1000);

            Client = new LinkClient();
            Client.Setup("127.0.0.1", 8080);
            TestContract = Client.GetContract<ITestContract>();
        }

        [TearDown]
        public void TearDown()
        {
            Client.Dispose();
            Server.Dispose();

            Client = null;
            Server = null;
        }
    }
}
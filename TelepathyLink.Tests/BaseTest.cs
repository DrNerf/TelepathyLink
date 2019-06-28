using System;
using System.Threading;
using TelepathyLink.Core;

namespace TelepathyLink.Tests
{
    public class BaseTest : IDisposable
    {
        protected LinkServer Server;
        protected LinkClient Client;
        protected ITestContract TestContract;
        protected TestContract TestContractInstance;

        public BaseTest()
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

        public void Dispose()
        {
            //TODO: Disconnect the client and shut down the server.
            Server = null;
            Client = null;
        }
    }
}

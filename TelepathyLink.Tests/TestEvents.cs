using NUnit.Framework;
using System.Threading.Tasks;

namespace TelepathyLink.Tests
{
    public class TestEvents : TestBase
    {
        [Test]
        public async Task TestSimpleEvent()
        {
            var receivedCallback = false;
            TestContract.TestEventHandler.Subscribe(() => { receivedCallback = true; });
            await Task.Delay(1000);
            TestContractInstance.TestEventHandler.Publish();
            await Task.Delay(1000);
            Assert.IsTrue(receivedCallback);
        }
    }
}
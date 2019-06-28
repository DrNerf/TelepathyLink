using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TelepathyLink.Tests
{
    public class TestEvents : BaseTest
    {
        [Fact]
        public async void TestSimpleEvent()
        {
            var receivedCallback = false;
            TestContract.TestEventHandler.Subscribe(() => { receivedCallback = true; });
            await Task.Delay(1000);
            TestContractInstance.TestEventHandler.Publish();
            await Task.Delay(1000);
            Assert.True(receivedCallback);
        }
    }
}

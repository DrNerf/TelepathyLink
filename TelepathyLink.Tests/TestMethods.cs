using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelepathyLink.Core;

namespace TelepathyLink.Tests
{
    public class TestMethods : TestBase
    {
        [Test]
        public Task SimpleMethod()
        {
            return Task.Run(action: TestContract.SimpleMethod).TimeoutAfter(10000);
        }
        
        [TestCase(2, 2, 4)]
        [TestCase(3, 2, 5)]
        [TestCase(200000, 200000, 400000)]
        public void ParamsAndReturnValue(int left, int right, int expected)
        {
            Assert.AreEqual(expected, TestContract.Add(left, right));
        }
        
        [TestCase(new int[] { 1, 0, 2, 5 }, 8)]
        [TestCase(new int[] { 10, 15, 20, 55 }, 100)]
        [TestCase(new int[] { 2000, 50000 }, 52000)]
        public void CollectionParam(IEnumerable<int> numbers, int expected)
        {
            Assert.AreEqual(expected, TestContract.Sum(numbers.ToArray()));
            Assert.AreEqual(expected, TestContract.Sum(numbers.ToList()));
            Assert.AreEqual(expected, TestContract.Sum(numbers.ToHashSet()));
        }
        
        [TestCase(2, 2, 4)]
        [TestCase(3, 2, 5)]
        [TestCase(200000, 200000, 400000)]
        [TestCase(2, null, 2)]
        [TestCase(null, 2, 2)]
        [TestCase(null, null, null)]
        public void NullableParamsAndReturnValue(int? left, int? right, int? expected)
        {
            Assert.AreEqual(expected, TestContract.AddNullables(left, right));
        }

        [Test]
        public void Overload()
        {
            Assert.AreEqual(8, TestContract.Sum(new int?[] { 1, 0, 2, 5 }));
            Assert.AreEqual(100, TestContract.Sum(new int?[] { 10, 15, 20, 55 }));
            Assert.AreEqual(52000, TestContract.Sum(new int?[] { 2000, 50000 }));
            Assert.AreEqual(50000, TestContract.Sum(new int?[] { null, 50000 }));
            Assert.AreEqual(0, TestContract.Sum(new int?[] { null, null }));
        }

        [Test]
        public async Task ConnectionId()
        {
            // The connection Id is global, so we don't have a reliable way to check if the value is correct
            // as the connection counter is shared between Server instances.
            Assert.IsNull(TestContractInstance.LatestConnectionId);
            TestContract.SimpleMethod();
            await Task.Delay(1000);
            Assert.IsNotNull(TestContractInstance.LatestConnectionId);
        }

        [Test]
        public async Task ConnectionIdMultipleClients()
        {
            // The connection Id is global, so we don't have a reliable way to check if the value is correct
            // as the connection counter is shared between Server instances.
            Assert.IsNull(TestContractInstance.LatestConnectionId);
            TestContract.SimpleMethod();
            var connId = TestContractInstance.LatestConnectionId;
            Assert.IsNotNull(connId);
            using (var client = new LinkClient())
            {
                client.Setup("127.0.0.1", 8080);
                await Task.Delay(1000);
                client.GetContract<ITestContract>().SimpleMethod();
                await Task.Delay(1000);
                Assert.AreNotEqual(connId, TestContractInstance.LatestConnectionId); 
            }
        }
    }
}
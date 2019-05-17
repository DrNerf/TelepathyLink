using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace TelepathyLink.Tests
{
    public class TestMethods : BaseTest
    {
        [Fact]
        public Task TestSimpleMethod()
        {
            return Task.Run(action: TestContract.SimpleMethod).TimeoutAfter(10000);
        }

        [Theory]
        [InlineData(2, 2, 4)]
        [InlineData(3, 2, 5)]
        [InlineData(200000, 200000, 400000)]
        public void TestParamsAndReturnValue(int left, int right, int expected)
        {
            Assert.Equal(expected, TestContract.Add(left, right));
        }

        [Theory]
        [InlineData(new int[] { 1, 0, 2, 5 }, 8)]
        [InlineData(new int[] { 10, 15, 20, 55 }, 100)]
        [InlineData(new int[] { 2000, 50000 }, 52000)]
        public void TestCollectionParam(IEnumerable<int> numbers, int expected)
        {
            Assert.Equal(expected, TestContract.Sum(numbers.ToArray()));
            Assert.Equal(expected, TestContract.Sum(numbers.ToList()));
            Assert.Equal(expected, TestContract.Sum(numbers.ToHashSet()));
        }
    }
}

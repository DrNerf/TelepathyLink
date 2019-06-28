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
        public Task SimpleMethod()
        {
            return Task.Run(action: TestContract.SimpleMethod).TimeoutAfter(10000);
        }

        [Theory]
        [InlineData(2, 2, 4)]
        [InlineData(3, 2, 5)]
        [InlineData(200000, 200000, 400000)]
        public void ParamsAndReturnValue(int left, int right, int expected)
        {
            Assert.Equal(expected, TestContract.Add(left, right));
        }

        [Theory]
        [InlineData(new int[] { 1, 0, 2, 5 }, 8)]
        [InlineData(new int[] { 10, 15, 20, 55 }, 100)]
        [InlineData(new int[] { 2000, 50000 }, 52000)]
        public void CollectionParam(IEnumerable<int> numbers, int expected)
        {
            Assert.Equal(expected, TestContract.Sum(numbers.ToArray()));
            Assert.Equal(expected, TestContract.Sum(numbers.ToList()));
            Assert.Equal(expected, TestContract.Sum(numbers.ToHashSet()));
        }

        [Theory]
        [InlineData(2, 2, 4)]
        [InlineData(3, 2, 5)]
        [InlineData(200000, 200000, 400000)]
        [InlineData(2, null, 2)]
        [InlineData(null, 2, 2)]
        [InlineData(null, null, null)]
        public void NullableParamsAndReturnValue(int? left, int? right, int? expected)
        {
            Assert.Equal(expected, TestContract.AddNullables(left, right));
        }

        [Fact]
        public void Overload()
        {
            Assert.Equal(8, TestContract.Sum(new int?[] { 1, 0, 2, 5 }));
            Assert.Equal(100, TestContract.Sum(new int?[] { 10, 15, 20, 55 }));
            Assert.Equal(52000, TestContract.Sum(new int?[] { 2000, 50000 }));
            Assert.Equal(50000, TestContract.Sum(new int?[] { null, 50000 }));
            Assert.Equal(0, TestContract.Sum(new int?[] { null, null }));
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using TelepathyLink.Core;
using TelepathyLink.Core.Attributes;

namespace TelepathyLink.Tests
{
    [Contract]
    public interface ITestContract
    {
        void SimpleMethod();

        int Add(int left, int right);

        int Sum(IEnumerable<int> numbers);
    }

    [Implementation]
    public class TestContract : ITestContract
    {
        public void SimpleMethod()
        {
        }

        public int Add(int left, int right)
        {
            return left + right;
        }

        public int Sum(IEnumerable<int> numbers)
        {
            return numbers.Sum();
        }
    }
}

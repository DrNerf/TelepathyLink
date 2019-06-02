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

        int Sum(IEnumerable<int?> numbers);

        int? AddNullables(int? left, int? right);
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

        public int? AddNullables(int? left, int? right)
        {
            if (!left.HasValue && !right.HasValue)
            {
                return null;
            }

            return (left ?? 0) + (right ?? 0);
        }

        public int Sum(IEnumerable<int?> numbers)
        {
            return numbers.Sum() ?? 0;
        }
    }
}

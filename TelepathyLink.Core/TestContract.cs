using TelepathyLink.Core.Attributes;

namespace TelepathyLink.Core
{
    [Implementation]
    public class TestContract : ITestContract
    {
        public static TestContract Instance;

        public ILinkedEventHandler StrikeBack { get; set; }

        public TestContract()
        {
            Instance = this;
        }

        public string Marco()
        {
            return "Polo!";
        }
    }

    [Contract]
    public interface ITestContract
    {
        string Marco();

        ILinkedEventHandler StrikeBack { get; set; }
    }
}

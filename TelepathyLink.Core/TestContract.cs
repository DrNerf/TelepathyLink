using TelepathyLink.Core.Attributes;

namespace TelepathyLink.Core
{
    [Implementation]
    public class TestContract : ITestContract
    {
        public LinkedEventHandler StrikeBack { get; set; }

        public string Marco()
        {
            return "Polo!";
        }
    }

    [Contract]
    public interface ITestContract
    {
        string Marco();

        LinkedEventHandler StrikeBack { get; set; }
    }
}

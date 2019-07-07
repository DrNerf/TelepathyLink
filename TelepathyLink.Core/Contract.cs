using TelepathyLink.Core.Models;

namespace TelepathyLink.Core
{
    public abstract class Contract
    {
        protected RequestModel LatestRequest { get; set; }
    }
}

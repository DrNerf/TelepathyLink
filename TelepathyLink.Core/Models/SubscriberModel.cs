using System;

namespace TelepathyLink.Core.Models
{
    internal class SubscriberModel
    {
        public int ClientId { get; set; }

        public Guid TransportIdentifier { get; set; }

        public SubscriberModel(int clientId, Guid transportIdentifier)
        {
            ClientId = clientId;
            TransportIdentifier = transportIdentifier;
        }
    }
}

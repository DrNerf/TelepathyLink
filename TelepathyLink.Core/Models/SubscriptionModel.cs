using System;

namespace TelepathyLink.Core.Models
{
    public class SubscriptionModel
    {
        public string Contract { get; set; }

        public string EventHandler { get; set; }

        public Type ParameterType { get; set; }
    }
}

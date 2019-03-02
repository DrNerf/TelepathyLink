using System;
using System.Collections.Generic;

namespace TelepathyLink.Core.Models
{
    internal class TransportModel
    {
        public Guid Identifier { get; set; }

        public string Contract { get; set; }

        public string Method { get; set; }

        public IDictionary<string, object> Parameters { get; set; }
    }
}
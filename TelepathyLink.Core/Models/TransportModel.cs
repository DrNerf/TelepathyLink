using System;
using System.Collections.Generic;

namespace TelepathyLink.Core.Models
{
    public class TransportModel
    {
        public Guid Identifier { get; set; }

        public string Contract { get; set; }

        public string Method { get; set; }

        public object[] Parameters { get; set; }

        public object ReturnValue { get; set; }
    }
}
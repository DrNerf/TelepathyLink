using System;

namespace TelepathyLink.Core.Models
{
    public class InvocationModel
    {
        public string Contract { get; set; }

        public string Method { get; set; }

        public object[] Arguments { get; set; }

        public Type ReturnType { get; set; }
    }
}

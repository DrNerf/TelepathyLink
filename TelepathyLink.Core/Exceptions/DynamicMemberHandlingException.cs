using System;

namespace TelepathyLink.Core.Exceptions
{
    public class DynamicMemberHandlingException : NotImplementedException
    {
        public DynamicMemberHandlingException()
            : base("Call to the dynamic member is not handled. This exception signals there is a contract mismatch between the client and the server.")
        {
        }

        public DynamicMemberHandlingException(string message)
            : base(message)
        {
        }
    }
}

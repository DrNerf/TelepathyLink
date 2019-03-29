using System;

namespace TelepathyLink.Core.Exceptions
{
    public class DynamicMemberHandlingException : NotImplementedException
    {
        public DynamicMemberHandlingException()
            : base("Call to the dynamic member is not handled. If you see this exception please reach out for support.")
        {
        }

        public DynamicMemberHandlingException(string message)
            : base(message)
        {
        }
    }
}

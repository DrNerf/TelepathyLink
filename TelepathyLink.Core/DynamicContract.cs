using System;
using System.Dynamic;
using TelepathyLink.Core.Models;

namespace TelepathyLink.Core
{
    internal class DynamicContract : DynamicObject
    {
        public Func<InvocationModel, object> InvokeMember { get; set; }
        public string Contract { get; set; }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var model = new InvocationModel()
            {
                Contract = Contract,
                Method = binder.Name,
                ReturnType = binder.ReturnType,
                Arguments = args
            };

            result = InvokeMember.Invoke(model);
            return true;
        }
    }
}

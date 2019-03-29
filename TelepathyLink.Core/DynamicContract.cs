using System;
using System.Dynamic;
using TelepathyLink.Core.Models;

namespace TelepathyLink.Core
{
    internal class DynamicContract : DynamicObject
    {
        public Func<InvocationModel, object> InvokeMember { get; set; }
        public string Contract { get; set; }

        public event EventHandler<SubscriptionModel> SubscribeToEvent;

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

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            //SubscribeToEvent?.Invoke(this, new SubscriptionModel()
            //{
            //    Contract = Contract,
            //    EventHandler = binder.Name,
            //    ParameterType = null // @ST Looks like this is not needed for now.
            //});
            result = new LinkedEventHandler();
            return true;
        }
    }
}

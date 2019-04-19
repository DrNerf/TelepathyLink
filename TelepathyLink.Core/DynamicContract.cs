using System;
using System.Dynamic;
using TelepathyLink.Core.Models;

namespace TelepathyLink.Core
{
    internal class DynamicContract : DynamicObject
    {
        public Func<InvocationModel, object> InvokeMember { get; set; }
        public Action<SubscriptionModel, Delegate> SubscribeToEvent { get; set; }
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

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            //SubscribeToEvent?.Invoke(this, new SubscriptionModel()
            //{
            //    Contract = Contract,
            //    EventHandler = binder.Name,
            //    ParameterType = null // @ST Looks like this is not needed for now.
            //});
            var handler = new LinkedEventHandler();
            handler.Published += (sender, client) => OnSubscribed(client, binder.Name, null, null);
            handler.PublishedWithCallback += (sender, data) => OnSubscribed(data.Item1, binder.Name, null, data.Item2);

            result = handler;
            return true;
        }

        private void OnSubscribed(int clientId, string handler, Type paramType, Delegate callback)
        {
            var model = new SubscriptionModel()
            {
                Contract = Contract,
                EventHandler = handler,
                //ParameterType = paramType @ST Not needed for now.
            };

            SubscribeToEvent.Invoke(model, callback);
        }
    }
}

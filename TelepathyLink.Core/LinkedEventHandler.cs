using System;
using TelepathyLink.Core.Models;

namespace TelepathyLink.Core
{
    public interface ILinkedEventHandler
    {
        void Subscribe(Action callback);
        void Subscribe<TParameter>(Action<TParameter> callback);
        void Publish();
        void Publish<TParameter>(TParameter param);
        void Publish(int clientId);
        void Publish<TParameter>(int clientId, TParameter param);
    }

    internal class LinkedEventClientHandler : ILinkedEventHandler
    {
        public event EventHandler<Delegate> Subscribed;
        public event EventHandler<int> Published;
        public event EventHandler<Tuple<int, object>> PublishedWithCallbackParam;

        public virtual void Subscribe(Action callback)
        {
            Subscribed?.Invoke(this, callback);
        }

        public virtual void Publish(int clientId)
        {
            throw new NotImplementedException();
        }

        public virtual void Subscribe<TParameter>(Action<TParameter> callback)
        {
            Subscribed?.Invoke(this, callback);
        }

        public virtual void Publish<TParameter>(int clientId, TParameter param)
        {
            throw new NotImplementedException();
        }

        public void Publish()
        {
            throw new NotImplementedException();
        }

        public void Publish<TParameter>(TParameter param)
        {
            throw new NotImplementedException();
        }
    }

    internal class LinkedEventServerHandler : ILinkedEventHandler
    {
        public string ContractName { get; set; }
        public string HandlerName { get; set; }
        public LinkServer Server { get; set; }

        public LinkedEventServerHandler(string contractName, string handlerName, LinkServer server)
        {
            ContractName = contractName;
            HandlerName = handlerName;
            Server = server;
        }

        public void Subscribe(Action callback)
        {
            throw new NotImplementedException();
        }

        public void Subscribe<TParameter>(Action<TParameter> callback)
        {
            throw new NotImplementedException();
        }

        public void Publish(int clientId)
        {
            var model = new SubscriptionModel()
            {
                Contract = ContractName,
                EventHandler = HandlerName,
                ParameterType = null // @ST not needed for now.
            };

            Server.PublishEvent<object>(clientId, null, model, null);
        }

        public void Publish<TParameter>(int clientId, TParameter param)
        {
            var model = new SubscriptionModel()
            {
                Contract = ContractName,
                EventHandler = HandlerName,
                ParameterType = null // @ST not needed for now.
            };

            Server.PublishEvent(clientId, param, model, null);
        }

        public void Publish()
        {
            var model = new SubscriptionModel()
            {
                Contract = ContractName,
                EventHandler = HandlerName,
                ParameterType = null // @ST not needed for now.
            };

            Server.PublishEvent<object>(null, model);
        }

        public void Publish<TParameter>(TParameter param)
        {
            var model = new SubscriptionModel()
            {
                Contract = ContractName,
                EventHandler = HandlerName,
                ParameterType = null // @ST not needed for now.
            };

            Server.PublishEvent(param, model);
        }
    }
}
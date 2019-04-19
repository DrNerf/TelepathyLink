using System;

namespace TelepathyLink.Core
{
    public interface ILinkedEventHandler
    {
        void Subscribe(Action callback);
        void Publish(int clientId);
        void Subscribe<TParameter>(Action<TParameter> callback);
        void Publish<TParameter>(int clientId, TParameter param);
    }

    internal class LinkedEventHandler : ILinkedEventHandler
    {
        public event EventHandler<Delegate> Subscribed;
        public event EventHandler<int> Published;
        public event EventHandler<Tuple<int, object>> PublishedWithCallback;

        public virtual void Subscribe(Action callback)
        {
            Subscribed?.Invoke(this, callback);
        }

        public virtual void Publish(int clientId)
        {
            Published?.Invoke(this, clientId);
        }

        public virtual void Subscribe<TParameter>(Action<TParameter> callback)
        {
            Subscribed?.Invoke(this, callback);
        }

        public virtual void Publish<TParameter>(int clientId, TParameter param)
        {
            PublishedWithCallback?.Invoke(this, new Tuple<int, object>(clientId, param));
        }
    }
}
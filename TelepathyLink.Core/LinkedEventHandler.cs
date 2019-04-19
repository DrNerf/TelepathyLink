using System;

namespace TelepathyLink.Core
{
    public class LinkedEventHandler
    {
        public event EventHandler<Delegate> Subscribed;
        public event EventHandler<int> Published;
        public event EventHandler<Tuple<int, Delegate>> PublishedWithCallback;

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

        public virtual void Publish<TParameter>(int clientId, Action<TParameter> param)
        {
            PublishedWithCallback?.Invoke(this, new Tuple<int, Delegate>(clientId, param));
        }
    }
}

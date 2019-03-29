using System;

namespace TelepathyLink.Core
{
    public class LinkedEventHandler
    {
        public event EventHandler<Delegate> Subscribed;
        public event EventHandler<int> Published;

        public virtual void Subscribe(Action callback)
        {
            Subscribed?.Invoke(this, callback);
        }

        public virtual void Publish(int clientId)
        {
            Published?.Invoke(this, clientId);
        }
    }

    public class LinkedEventHandler<TParameter>
    {
        public event EventHandler<Delegate> Subscribed;
        public event EventHandler<Tuple<int, Delegate>> Published;

        public virtual void Subscribe(Action<TParameter> callback)
        {
            Subscribed?.Invoke(this, callback);
        }

        public virtual void Publish(int clientId, Action<TParameter> param)
        {
            Published?.Invoke(this, new Tuple<int, Delegate>(clientId, param));
        }
    }
}

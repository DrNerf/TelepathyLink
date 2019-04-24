using ImpromptuInterface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Telepathy;
using TelepathyLink.Core.Attributes;
using TelepathyLink.Core.Models;

namespace TelepathyLink.Core
{
    public class LinkClient : AbstractLink
    {
        private IDictionary<Guid, TransportModel> m_PendingResponses;
        private IDictionary<Guid, Delegate> m_ActiveSubscriptions;

        public Client TelepathyClient { get; set; }

        public LinkClient()
        {
            TelepathyClient = new Client();
            m_PendingResponses = new ConcurrentDictionary<Guid, TransportModel>();
            m_ActiveSubscriptions = new ConcurrentDictionary<Guid, Delegate>();
            TelepathyTcpCommonClient = TelepathyClient;
        }

        public TContract GetContract<TContract>()
            where TContract : class
        {
            var isContract = typeof(TContract).CustomAttributes.Any(ca => ca.AttributeType == typeof(ContractAttribute));
            if (isContract)
            {
                var dynamicContract = new DynamicContract();
                dynamicContract.InvokeMember = OnContractMethodInvoked;
                dynamicContract.SubscribeToEvent = OnSubscribeToEvent;
                dynamicContract.Contract = typeof(TContract).FullName;
                return Impromptu.ActLike<TContract>(dynamicContract);
            }
            else
            {
                throw new InvalidOperationException($"{typeof(TContract).ToString()} is not a Telepathy Link contract. Try decorating it with the Contract attribute.");
            }
        }

        public void Setup(string ip, int port, int listeningInterval = 200)
        {
            TelepathyClient.Connect(ip, port);
            StartListening(listeningInterval);
        }

        public void OnSubscribeToEvent(SubscriptionModel model, Delegate callback)
        {
            var transport = new TransportModel()
            {
                Contract = model.Contract,
                Identifier = Guid.NewGuid(),
                Type = TransportType.Event,
                EventHandler = model.EventHandler
            };

            m_ActiveSubscriptions.Add(transport.Identifier, callback);
            TelepathyClient.Send(SerializeTransport(transport));
        }

        protected override void OnMessageReceived(Message msg)
        {
            var transport = DeserializeTransport<TransportModel>(msg.data);
            if (transport.Type == TransportType.Method)
            {
                if (m_PendingResponses.ContainsKey(transport.Identifier))
                {
                    m_PendingResponses.Remove(transport.Identifier);
                }

                m_PendingResponses.Add(transport.Identifier, transport); 
            }
            else
            {
                if (m_ActiveSubscriptions.TryGetValue(transport.Identifier, out var callback))
                {
                    callback.DynamicInvoke(transport.Parameters);
                }
            }
        }

        private object OnContractMethodInvoked(InvocationModel model)
        {
            var transport = new TransportModel()
            {
                Identifier = Guid.NewGuid(),
                Contract = model.Contract,
                Method = model.Method,
                Parameters = model.Arguments,
                Type = TransportType.Method
            };
            
            TelepathyClient.Send(SerializeTransport(transport));
            var response = WaitForReponse(transport.Identifier);

            return response.ReturnValue;
        }

        private TransportModel WaitForReponse(Guid identifier)
        {
            TransportModel transport;

            // Lets block the thread while waiting.
            while (!ResponseAvailable(identifier, out transport))
            {
                // Do nothing.
            }

            return transport;
        }

        private bool ResponseAvailable(Guid identifier, out TransportModel response)
        {
            var available = m_PendingResponses.ContainsKey(identifier);
            response = available ? m_PendingResponses[identifier] : null;

            return available;
        }
    }
}

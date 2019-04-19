using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Telepathy;
using TelepathyLink.Core.Attributes;
using TelepathyLink.Core.Models;

namespace TelepathyLink.Core
{
    public class LinkServer : AbstractLink
    {
        private IDictionary<string, object> m_Contracts;

        public Server TelepathyServer { get; set; }

        public LinkServer()
        {
            m_Contracts = new ConcurrentDictionary<string, object>();
            TelepathyServer = new Server();
            TelepathyTcpCommonClient = TelepathyServer;
        }

        public void RegisterContracts()
        {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes());
            var contracts = allTypes.Where(t => t.CustomAttributes.Any(ca => ca.AttributeType == typeof(ContractAttribute)));
            var contractImplementations = new Dictionary<Type, Type>();
            foreach (var contract in contracts)
            {
                var implementations = allTypes.Where(
                    t => t.GetInterfaces().Contains(contract) && t.CustomAttributes.Any(ca => ca.AttributeType == typeof(ImplementationAttribute)));
                if (!implementations.Any())
                {
                    throw new Exception($"Could not find implementation class for contract {contract.Name}. Try decorating it with the Implementation attribute.");
                }
                else if (implementations.Count() > 1)
                {
                    throw new Exception($"Multiple implementations found for contract {contract.Name}.");
                }

                contractImplementations.Add(contract, implementations.First());
            }

            RegisterContracts(contractImplementations);
        }

        public void RegisterContracts(IEnumerable<KeyValuePair<Type, Type>> contracts)
        {
            foreach (var contract in contracts)
            {
                ValidateTypeIsContract(contract.Key);
                var impl = contract.Value.GetConstructor(Type.EmptyTypes).Invoke(null);
                var properties = contract.Value.GetProperties();
                foreach (var prop in properties)
                {
                    if (prop.PropertyType == typeof(ILinkedEventHandler))
                    {
                        prop.SetValue(impl, new LinkedEventHandler());
                    }
                }
                m_Contracts.Add(contract.Key.FullName, impl);
            }
        }

        public void RegisterContractImplementation<TContract, TImplementation>(TImplementation impl)
            where TContract : class
            where TImplementation : TContract
        {
            ValidateTypeIsContract(typeof(TContract));
            m_Contracts.Add(typeof(TContract).FullName, impl);
        }

        public void Start(int port, int listeningInterval = 200)
        {
            TelepathyServer.Start(port);
            StartListening(listeningInterval, OnMessageReceived);
        }

        public void PublishEvent<TParameter>(int clientId, TParameter param, SubscriptionModel model)
        {

        }

        private void OnMessageReceived(Message message)
        {
            var transport = DeserializeTransport<TransportModel>(message.data);
            if (m_Contracts.TryGetValue(transport.Contract, out var contract))
            {
                var contractType = contract.GetType();
                if (transport.Type == TransportType.Method)
                {
                    var result = contractType.InvokeMember(
                                transport.Method,
                                BindingFlags.InvokeMethod,
                                Type.DefaultBinder,
                                contract,
                                transport.Parameters);
                    transport.ReturnValue = result;
                    TelepathyServer.Send(message.connectionId, SerializeTransport(transport));
                }
                else
                {
                    var handler = contractType.GetEvent(transport.EventHandler);
                    handler.AddEventHandler(
                        contract,
                        GetEventCallback(handler, contract, message.connectionId, transport));
                }
            }
            else
            {
                //TODO: I dunno, handle this somehow.
            }
        }

        private Delegate GetEventCallback(
            EventInfo eventInfo,
            object contract,
            int connectionId,
            TransportModel transport)
        {
            return new Action<int, object>((clientId, param) =>
            {
                if (clientId == connectionId)
                {
                    transport.Parameters = new object[] { param };
                    TelepathyServer.Send(connectionId, SerializeTransport(transport));
                }
            });
        }
    }
}

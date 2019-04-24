using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Telepathy;
using TelepathyLink.Core.Attributes;
using TelepathyLink.Core.Exceptions;
using TelepathyLink.Core.Models;

namespace TelepathyLink.Core
{
    public class LinkServer : AbstractLink
    {
        private readonly IDictionary<string, object> contracts;
        private readonly IDictionary<string, IList<Tuple<int, Guid>>> subscriptions;

        public Server TelepathyServer { get; set; }

        public LinkServer()
        {
            contracts = new ConcurrentDictionary<string, object>();
            subscriptions = new ConcurrentDictionary<string, IList<Tuple<int, Guid>>>();
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
                        var handler = new LinkedEventServerHandler(
                            contract.Key.Name,
                            prop.Name,
                            this);

                        prop.SetValue(impl, handler);
                    }
                }

                this.contracts.Add(contract.Key.FullName, impl);
            }
        }

        public void RegisterContractImplementation<TContract, TImplementation>(TImplementation impl)
            where TContract : class
            where TImplementation : TContract
        {
            ValidateTypeIsContract(typeof(TContract));
            contracts.Add(typeof(TContract).FullName, impl);
        }

        public void Start(int port, int listeningInterval = 200)
        {
            TelepathyServer.Start(port);
            StartListening(listeningInterval);
        }

        public void PublishEvent<TParameter>(int clientId, TParameter param, SubscriptionModel model, Guid? identifier)
        {
            var transport = new TransportModel()
            {
                Contract = model.Contract,
                EventHandler = model.EventHandler,
                Type = TransportType.Event,
                Parameters = new object[] { param }
            };

            if (!identifier.HasValue)
            {
                var subscribers = subscriptions[GetSubsciptionIdentifier(transport)];
                var clientSubscription = subscribers.First(s => s.Item1 == clientId);
                identifier = clientSubscription.Item2;
            }

            transport.Identifier = identifier.Value;
            TelepathyServer.Send(clientId, SerializeTransport(transport));
        }

        public void PublishEvent<TParameter>(TParameter param, SubscriptionModel model)
        {
            var transport = new TransportModel()
            {
                Contract = model.Contract,
                EventHandler = model.EventHandler,
                Type = TransportType.Event,
                Parameters = new object[] { param }
            };
            
            var subscribers = subscriptions[GetSubsciptionIdentifier(transport)];
            if (subscriptions.TryGetValue(GetSubsciptionIdentifier(transport), out var clients))
            {
                foreach (var client in clients)
                {
                    transport.Identifier = client.Item2;
                    TelepathyServer.Send(client.Item1, SerializeTransport(transport));
                }
            }
        }

        protected override void OnMessageReceived(Message message)
        {
            var transport = DeserializeTransport<TransportModel>(message.data);
            if (contracts.TryGetValue(transport.Contract, out var contract))
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
                    var handlerInfo = contractType.GetProperty(transport.EventHandler);
                    var handler = handlerInfo.GetValue(contract) as ILinkedEventHandler;
                    if (handler == null)
                    {
                        throw new DynamicMemberHandlingException();
                    }

                    var subIdentifier = GetSubsciptionIdentifier(transport);
                    if (subscriptions.TryGetValue(subIdentifier, out var subscribers))
                    {
                        subscribers.Add(new Tuple<int, Guid>(message.connectionId, transport.Identifier));
                    }
                    else
                    {
                        var subscription = new List<Tuple<int, Guid>>()
                        {
                            new Tuple<int, Guid>(message.connectionId, transport.Identifier)
                        };

                        subscriptions.Add(subIdentifier, subscription);
                    }
                }
            }
            else
            {
                //TODO: I dunno, handle this somehow.
            }
        }

        private string GetSubsciptionIdentifier(TransportModel model)
        {
            return $"{model.Contract} {model.EventHandler}";
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

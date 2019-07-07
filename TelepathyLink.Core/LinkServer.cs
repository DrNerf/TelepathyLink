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
        private readonly IDictionary<string, IList<SubscriberModel>> subscriptions;

        public Server TelepathyServer { get; set; }

        public LinkServer()
        {
            contracts = new ConcurrentDictionary<string, object>();
            subscriptions = new ConcurrentDictionary<string, IList<SubscriberModel>>();
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
                FillSystemProperties(contract.Value, contract.Key, ref impl);

                this.contracts.Add(contract.Key.FullName, impl);
            }
        }

        public void RegisterContractImplementation<TContract, TImplementation>(TImplementation impl)
            where TContract : class
            where TImplementation : TContract
        {
            var implObject = impl as object;
            ValidateTypeIsContract(typeof(TContract));
            FillSystemProperties(typeof(TImplementation), typeof(TContract), ref implObject);

            contracts.Add(typeof(TContract).FullName, impl);
        }

        public void Start(int port, int listeningInterval = 200)
        {
            TelepathyServer.Start(port);
            StartListening(listeningInterval);
        }

        public void PublishEvent(object param, bool hasParams, SubscriptionModel model)
        {
            if (subscriptions.TryGetValue(GetSubsciptionIdentifier(model), out var clients))
            {
                foreach (var client in clients)
                {
                    PublishEvent(client.ClientId, client.TransportIdentifier, param, hasParams, model);
                }
            }
        }

        public void PublishEvent(int clientId, object param, bool hasParams, SubscriptionModel model)
        {
            if (subscriptions.TryGetValue(GetSubsciptionIdentifier(model), out var clients))
            {
                var client = clients.FirstOrDefault(c => c.ClientId == clientId);
                if (client != null)
                {
                    PublishEvent(client.ClientId, client.TransportIdentifier, param, hasParams, model);
                }
            }
        }

        public void PublishEvent(int clientId, Guid identifier, object param, bool hasParams, SubscriptionModel model)
        {
            var transport = new TransportModel()
            {
                Contract = model.Contract,
                EventHandler = model.EventHandler,
                Type = TransportType.Event,
                Parameters = hasParams ? new object[] { param } : Array.Empty<object>(),
                Identifier = identifier
            };
            
            TelepathyServer.Send(clientId, SerializeTransport(transport));
        }

        protected override void OnMessageReceived(Message message)
        {
            var transport = DeserializeTransport<TransportModel>(message.data);
            if (contracts.TryGetValue(transport.Contract, out var contract))
            {
                var contractType = contract.GetType();
                if (transport.Type == TransportType.Method)
                {
                    SetLatestRequest(
                        new RequestModel() { ConnectionId = message.connectionId },
                        contract,
                        contractType);
                    var result = contractType.InvokeMember(
                                transport.Method,
                                BindingFlags.InvokeMethod,
                                Type.DefaultBinder,
                                contract,
                                transport.Parameters);
                    transport.ReturnValue = result;
                    SetLatestRequest(null, contract, contractType);
                    TelepathyServer.Send(message.connectionId, SerializeTransport(transport));
                }
                else
                {
                    var handlerInfo = contractType.GetProperty(transport.EventHandler);
                    if (!(handlerInfo.GetValue(contract) is ILinkedEventHandler handler))
                    {
                        throw new DynamicMemberHandlingException();
                    }

                    var subIdentifier = GetSubsciptionIdentifier(transport);
                    if (subscriptions.TryGetValue(subIdentifier, out var subscribers))
                    {
                        subscribers.Add(new SubscriberModel(message.connectionId, transport.Identifier));
                    }
                    else
                    {
                        var subscription = new List<SubscriberModel>()
                        {
                            new SubscriberModel(message.connectionId, transport.Identifier)
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

        private void SetLatestRequest(RequestModel value, object contract, Type contractType)
        {
            var latestRequestInfo = contractType.GetProperty(
                "LatestRequest",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (latestRequestInfo != null && latestRequestInfo.PropertyType == typeof(RequestModel))
            {
                latestRequestInfo.SetValue(contract, value);
            }
        }

        private string GetSubsciptionIdentifier(TransportModel model)
        {
            return $"{model.Contract} {model.EventHandler}";
        }

        private string GetSubsciptionIdentifier(SubscriptionModel model)
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

        private void FillSystemProperties(Type implType, Type contractType, ref object impl)
        {
            var properties = implType.GetProperties();
            foreach (var prop in properties)
            {
                if (prop.PropertyType == typeof(ILinkedEventHandler))
                {
                    var handler = new LinkedEventServerHandler(
                        contractType.FullName,
                        prop.Name,
                        this);

                    prop.SetValue(impl, handler);
                }
            }
        }
    }
}

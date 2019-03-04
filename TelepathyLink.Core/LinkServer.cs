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

        public void RegisterContracts(IEnumerable<KeyValuePair<Type, Type>> contracts)
        {
            foreach (var contract in contracts)
            {
                ValidateTypeIsContract(contract.Value);
                var impl = contract.Value.GetConstructor(Type.EmptyTypes).Invoke(null);
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

        private void OnMessageReceived(Message message)
        {
            var transport = DeserializeTransport(message.data);
            if (m_Contracts.TryGetValue(transport.Contract, out var contract))
            {
                var contractType = contract.GetType();
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
                //TODO: I dunno, handle this somehow.
            }
        }
    }
}

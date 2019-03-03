using ImpromptuInterface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using Telepathy;
using TelepathyLink.Core.Attributes;
using TelepathyLink.Core.Models;

namespace TelepathyLink.Core
{
    public class LinkClient
    {
        private IDictionary<Guid, TransportModel> m_PendingResponses;

        public Client TelepathyClient { get; set; }

        public LinkClient()
        {
            TelepathyClient = new Client();
            m_PendingResponses = new ConcurrentDictionary<Guid, TransportModel>();
        }

        public TContract GetContract<TContract>()
            where TContract : class
        {
            var isContract = typeof(TContract).CustomAttributes.Any(ca => ca.AttributeType == typeof(ContractAttribute));
            if (isContract)
            {
                var dynamicContract = new DynamicContract();
                dynamicContract.InvokeMember = OnContractMethodInvoked;
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

        private void StartListening(int pollInterval)
        {
            new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    Message msg;
                    while (TelepathyClient.GetNextMessage(out msg))
                    {
                        switch (msg.eventType)
                        {
                            case EventType.Data:
                                Console.WriteLine("Data: " + BitConverter.ToString(msg.data));
                                OnDataReceived(msg);
                                break;
                        }
                    }

                    Thread.Sleep(pollInterval);
                }
            })).Start();
        }

        private void OnDataReceived(Message msg)
        {
            var serializer = new XmlSerializer(typeof(TransportModel));
            using (var reader = new StringReader(BitConverter.ToString(msg.data)))
            {
                var transport = serializer.Deserialize(reader) as TransportModel;
                if (m_PendingResponses.ContainsKey(transport.Identifier))
                {
                    m_PendingResponses.Remove(transport.Identifier);
                }

                m_PendingResponses.Add(transport.Identifier, transport);
            }
        }

        private object OnContractMethodInvoked(InvocationModel model)
        {
            var transport = new TransportModel()
            {
                Identifier = Guid.NewGuid(),
                Contract = model.Contract,
                Method = model.Method
            };
            var serializer = new XmlSerializer(transport.GetType());
            byte[] transportBytes;
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, transport);
                transportBytes = stream.ToArray();
            }

            TelepathyClient.Send(transportBytes);
            transport = WaitForReponse(transport);

            return transport.ReturnValue;
        }

        private TransportModel WaitForReponse(TransportModel transport)
        {
            // Lets block the thread while waiting.
            while (!ResponseAvailable(transport.Identifier, out transport))
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

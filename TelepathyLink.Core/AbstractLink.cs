using System;
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
    public abstract class AbstractLink
    {
        protected Common TelepathyTcpCommonClient;

        public event EventHandler<Message> MessageReceived;

        protected virtual void ValidateTypeIsContract(Type contract)
        {
            if (!contract.CustomAttributes.Any(ca => ca.AttributeType == typeof(ContractAttribute)))
            {
                throw new InvalidOperationException($"{contract.ToString()} is not a Telepathy Link contract. Try decorating it with the Contract attribute.");
            }
        }

        protected void StartListening(int pollInterval, Action<Message> newDataMessageReceived)
        {
            new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    Message msg;
                    while (TelepathyTcpCommonClient.GetNextMessage(out msg))
                    {
                        if (msg.eventType == EventType.Data)
                        {
                            newDataMessageReceived.Invoke(msg);
                        }

                        MessageReceived?.Invoke(this, msg);
                    }

                    Thread.Sleep(pollInterval);
                }
            })).Start();
        }

        protected byte[] SerializeTransport(TransportModel model)
        {
            var serializer = new XmlSerializer(model.GetType());
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, model);
                return stream.ToArray();
            }
        }

        protected TTransport DeserializeTransport<TTransport>(byte[] raw)
            where TTransport : class
        {
            var serializer = new XmlSerializer(typeof(TTransport));
            using (var reader = new StringReader(Encoding.UTF8.GetString(raw)))
            {
                return serializer.Deserialize(reader) as TTransport;
            }
        }
    }
}
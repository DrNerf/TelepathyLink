using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using Telepathy;
using TelepathyLink.Core.Attributes;
using TelepathyLink.Core.Models;

namespace TelepathyLink.Core
{
    public abstract class AbstractLink : IDisposable
    {
        protected Common TelepathyTcpCommonClient;

        public void Dispose()
        {
            if (TelepathyTcpCommonClient is Server)
            {
                (TelepathyTcpCommonClient as Server).Stop();
            }
            else
            {
                (TelepathyTcpCommonClient as Client).Disconnect();
            }
        }

        protected virtual void ValidateTypeIsContract(Type contract)
        {
            if (!contract.CustomAttributes.Any(ca => ca.AttributeType == typeof(ContractAttribute)))
            {
                throw new InvalidOperationException($"{contract.ToString()} is not a Telepathy Link contract. Try decorating it with the Contract attribute.");
            }
        }

        protected void StartListening(int pollInterval)
        {
            new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    while (TelepathyTcpCommonClient.GetNextMessage(out var msg))
                    {
                        if (msg.eventType == EventType.Data)
                        {
                            OnMessageReceived(msg);
                        }
                    }

                    Thread.Sleep(pollInterval);
                }
            })).Start();
        }

        protected byte[] SerializeTransport(TransportModel model)
        {
            var serializer = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, model);
                return stream.ToArray();
            }
        }

        protected TTransport DeserializeTransport<TTransport>(byte[] raw)
            where TTransport : class
        {
            var serializer = new BinaryFormatter();
            using (var stream = new MemoryStream(raw))
            {
                return serializer.Deserialize(stream) as TTransport;
            }
        }

        protected virtual void OnMessageReceived(Message message)
        {
        }
    }
}
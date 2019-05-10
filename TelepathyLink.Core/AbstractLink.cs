using Newtonsoft.Json;
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
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
        }

        protected TTransport DeserializeTransport<TTransport>(byte[] raw)
            where TTransport : class
        {
            return JsonConvert.DeserializeObject<TTransport>(Encoding.UTF8.GetString(raw));
        }

        protected virtual void OnMessageReceived(Message message)
        {
        }
    }
}
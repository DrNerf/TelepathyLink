using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using Telepathy;
using TelepathyLink.Core.Attributes;
using TelepathyLink.Core.Helpers;
using TelepathyLink.Core.Models;

namespace TelepathyLink.Core
{
    public abstract class AbstractLink
    {
        private JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.Indented
        };

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
            var dynamicTransport = TypeBuilderHelper.GetDynamicTransport(model);
            string json = JsonConvert.SerializeObject(dynamicTransport, serializerSettings);
            return Encoding.UTF8.GetBytes(json);
        }

        protected TTransport DeserializeTransport<TTransport>(byte[] raw)
            where TTransport : class
        {
            object transport = JsonConvert.DeserializeObject<dynamic>(
                Encoding.UTF8.GetString(raw),
                serializerSettings);
            return null;
        }

        protected virtual void OnMessageReceived(Message message)
        {
        }
    }
}
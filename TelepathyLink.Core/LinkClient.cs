using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using TelepathyLink.Core.Attributes;
using FakeItEasy;
using System.Reflection.Emit;

namespace TelepathyLink.Core
{
    public class LinkClient
    {
        public IDictionary<Type, object> Contracts { get; set; }

        public LinkClient()
        {
            Contracts = new Dictionary<Type, object>();
        }

        public void Initialize()
        {
            LoadContracts(Assembly.GetCallingAssembly());
        }

        private void LoadContracts(Assembly assembly)
        {
            var contracts = GetContractTypes(assembly);
            var builder = TypeBuilder.;
            foreach (var type in contracts)
            {
                var methods = type.GetMethods();
                foreach (var method in methods)
                {
                }
            }
        }

        private IEnumerable<Type> GetContractTypes(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(ContractAttribute), false).Length > 0)
                {
                    yield return type;
                }
            }
        }
    }
}

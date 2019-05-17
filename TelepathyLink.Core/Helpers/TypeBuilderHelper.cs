using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TelepathyLink.Core.Models;

namespace TelepathyLink.Core.Helpers
{
    public static class TypeBuilderHelper
    {
        private static readonly AssemblyName assembly;
        private static readonly ModuleBuilder moduleBuilder;

        static TypeBuilderHelper()
        {
            assembly = new AssemblyName("DynamicTransportAssembly");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName(Guid.NewGuid().ToString()), 
                AssemblyBuilderAccess.Run);

            moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicTransportModule");
        }

        public static object GetDynamicTransport<TTransportModel>(TTransportModel model)
            where TTransportModel : TransportModel
        {
            if (model.Parameters == null || !model.Parameters.Any())
            {
                // No parameters => we don't have to do anything.
                return model;
            }

            var typeBuilder = moduleBuilder.DefineType(Guid.NewGuid().ToString(), TypeAttributes.Public, model.GetType());
            for (int i = 0; i < model.Parameters.Length; i++)
            {
                var parameter = model.Parameters[i];
                var propertyName = $"Parameter{i}";
                DefineParameterProperty(ref typeBuilder, parameter, propertyName);
            }

            var dynamicTransportType = typeBuilder.CreateTypeInfo().AsType();
            var dynamicTransport = Activator.CreateInstance(dynamicTransportType);
            foreach (var propertyInfo in model.GetType().GetProperties())
            {
                // Map the old transport to the new dynamic one.
                if (propertyInfo.Name != "Parameters")
                {
                    var property = dynamicTransportType.GetProperty(propertyInfo.Name);
                    property.SetValue(dynamicTransport, property.GetValue(model)); 
                }
                else
                {
                    // Map the parameters array to the new properties.
                    for (int i = 0; i < model.Parameters.Length; i++)
                    {
                        var property = dynamicTransportType.GetProperty($"Parameter{i}");
                        property.SetValue(dynamicTransport, model.Parameters[i]);
                    }
                }
            }

            return dynamicTransport;
        }

        private static void DefineParameterProperty(ref TypeBuilder typeBuilder, object parameter, string propertyName)
        {
            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(
                                propertyName,
                                PropertyAttributes.None,
                                parameter.GetType(),
                                null);

            // Define field
            FieldBuilder fieldBuilder = typeBuilder.DefineField($"m_{propertyName}", parameter.GetType(), FieldAttributes.Private);
            // Define "getter" for MyChild property
            MethodBuilder getterBuilder = typeBuilder.DefineMethod($"get_{propertyName}",
                                                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                                                parameter.GetType(),
                                                Type.EmptyTypes);
            ILGenerator getterIL = getterBuilder.GetILGenerator();
            getterIL.Emit(OpCodes.Ldarg_0);
            getterIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getterIL.Emit(OpCodes.Ret);

            // Define "setter" for MyChild property
            MethodBuilder setterBuilder = typeBuilder.DefineMethod($"set_{propertyName}",
                                                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                                                null,
                                                new Type[] { parameter.GetType() });
            ILGenerator setterIL = setterBuilder.GetILGenerator();
            setterIL.Emit(OpCodes.Ldarg_0);
            setterIL.Emit(OpCodes.Ldarg_1);
            setterIL.Emit(OpCodes.Stfld, fieldBuilder);
            setterIL.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getterBuilder);
            propertyBuilder.SetSetMethod(setterBuilder);
        }
    }
}

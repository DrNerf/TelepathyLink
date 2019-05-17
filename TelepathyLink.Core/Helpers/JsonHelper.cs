using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace TelepathyLink.Core.Helpers
{
    public static class JsonHelper
    {
        private static readonly Type[] _specialNumericTypes = { typeof(ulong), typeof(uint), typeof(ushort), typeof(sbyte) };

        /// <summary>
        /// Converts values that were deserialized from JSON with weak typing (e.g. into <see cref="object"/>) back into
        /// their strong type, according to the specified target type.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The type the value should be converted into.</param>
        /// <returns>The converted value.</returns>
        public static object ConvertWeaklyTypedValue(object value, Type targetType)
        {
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            if (value == null)
                return null;

            if (targetType.IsInstanceOfType(value))
                return value;

            var paramType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (paramType.IsEnum)
            {
                if (value is string)
                    return Enum.Parse(paramType, (string)value);
                else
                    return Enum.ToObject(paramType, value);
            }

            if (paramType == typeof(Guid))
            {
                return Guid.Parse((string)value);
            }

            if (_specialNumericTypes.Contains(paramType))
            {
                if (value is BigInteger)
                    return (ulong)(BigInteger)value;
                else
                    return Convert.ChangeType(value, paramType);
            }

            if (value is long)
            {
                return Convert.ChangeType(value, paramType);
            }

            throw new ArgumentException($"Cannot convert a value of type {value.GetType()} to {targetType}.");
        }
    }
}

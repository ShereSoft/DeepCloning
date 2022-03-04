using System;
using System.Collections.Generic;
using System.Linq;

namespace ShereSoft
{
#if UNDER_DEVELOPMENT
    sealed class TypeNameResolver
    {
        internal readonly static Dictionary<Type, string> TypeNameTranslator = new Dictionary<Type, string>
        {
            { typeof(string), "string" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(double), "double" },
            { typeof(decimal), "decimal" },
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(char), "char" },
            { typeof(float), "float" },
            { typeof(short), "short" },
            { typeof(sbyte), "sbyte" },
            { typeof(uint), "uint" },
            { typeof(ulong), "ulong" },
            { typeof(ushort), "ushort" },
            { typeof(int?), "int?" },
            { typeof(long?), "long?" },
            { typeof(double?), "double?" },
            { typeof(decimal?), "decimal?" },
            { typeof(bool?), "bool?" },
            { typeof(byte?), "byte?" },
            { typeof(char?), "char?" },
            { typeof(float?), "float?" },
            { typeof(short?), "short?" },
            { typeof(sbyte?), "sbyte?" },
            { typeof(uint?), "uint?" },
            { typeof(ulong?), "ulong?" },
            { typeof(ushort?), "ushort?" },
        };

        public static string Resolve(Type type)
        {
            if (TypeNameTranslator.TryGetValue(type, out var name))
            {
                return name;
            }

            name = type.Name;

            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    name = $"{Resolve(type.GetGenericArguments()[0])}?";
                }
                else
                {
                    name = $"{name.Split('`')[0]}<{String.Join(",", type.GetGenericArguments().Select(a => Resolve(a)))}>";
                }
            }

            if (type.Namespace == null)
            {
                return name;
            }

            return $"{type.Namespace}.{name}";
        }
    }
#endif
}

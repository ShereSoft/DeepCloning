using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ShereSoft
{
    /// <summary>
    /// Provides common functionality and utilities for deep-cloning objects. This is an abstract class.
    /// </summary>
    public abstract class DeepCloning
    {
        internal delegate object CloneObjectDelegate(object src, Dictionary<object, object> objs, DeepCloningOptions options);

#if DEBUG
        internal protected readonly static ConcurrentDictionary<string, Type> CompiledMapperTypes = new ConcurrentDictionary<string, Type>();
#endif

#if UNDER_DEVELOPMENT
        internal readonly static HashSet<Type> DefaultUnclonableTypes = new HashSet<Type>(new[]
        {
            typeof(Type),

            typeof(Type).Assembly.Modules.SelectMany(m => m.GetTypes()).FirstOrDefault(t => t.Name == "RuntimeType"),
            typeof(TypeInfo),

            typeof(System.Globalization.CultureInfo),
            typeof(System.Globalization.NumberFormatInfo),
            typeof(System.Globalization.DateTimeFormatInfo),
            typeof(MemberInfo),
            typeof(PropertyInfo),
            typeof(MethodInfo),
            typeof(FieldInfo),
            typeof(ConstructorInfo),
            typeof(Assembly),
            typeof(Module),
        });
#endif

#if DEBUG
        public static Type[] GetCompiledClonerTypes()
        {
            return CompiledMapperTypes.ToArray().Select(kv => kv.Value).OrderBy(t => t.FullName).ToArray();
        }

        public static string[] GetCompiledClonerTypeNames()
        {
            return CompiledMapperTypes.ToArray().Select(kv => TypeNameResolver.Resolve(kv.Value)).OrderBy(n => n).ToArray();
        }
#endif

        internal static bool IsSimpleValueType(Type type)
        {
            if (!type.IsValueType)
            {
                return false;
            }

            if (type.IsPrimitive || type == typeof(decimal) || type.IsEnum || type == typeof(DateTime) || type == typeof(Guid))
            {
                return true;
            }

            var baseType = type;

            while (baseType != typeof(object))
            {
                foreach (var field in baseType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (!field.FieldType.IsValueType)
                    {
                        return false;
                    }
                }

                baseType = baseType.BaseType;
            }

            return true;
        }

        internal static CloneObjectDelegate BuildRedirect(Type type)
        {
            var method = new DynamicMethod(String.Empty, typeof(object), new Type[] { typeof(object), typeof(Dictionary<object, object>), typeof(DeepCloningOptions) });
            var il = method.GetILGenerator();

            if (type.IsValueType)
            {
                if (IsSimpleValueType(type))
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ret);

#if NET5_0_OR_GREATER
                    return method.CreateDelegate<CloneObjectDelegate>();
#else
                    return (CloneObjectDelegate)method.CreateDelegate(typeof(CloneObjectDelegate));            
#endif
                }
    
                il.Emit(OpCodes.Ldsfld, typeof(DeepCloning<>).MakeGenericType(type).GetField(nameof(DeepCloning<object>.CloneObject), BindingFlags.NonPublic | BindingFlags.Static));
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Unbox_Any, type);
            }
            else
            {
                il.Emit(OpCodes.Ldsfld, typeof(DeepCloning<>).MakeGenericType(type).GetField(nameof(DeepCloning<object>.CloneObject), BindingFlags.NonPublic | BindingFlags.Static));
                il.Emit(OpCodes.Ldarg_0);
            }

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Call, typeof(CloneObjectDelegate<>).MakeGenericType(type).GetMethod("Invoke"));

            if (type.IsValueType)
            {
                il.Emit(OpCodes.Box, type);
            }

            il.Emit(OpCodes.Ret);

#if NET5_0_OR_GREATER
            return method.CreateDelegate<CloneObjectDelegate>();
#else
            return (CloneObjectDelegate)method.CreateDelegate(typeof(CloneObjectDelegate));            
#endif
        }
    }
}

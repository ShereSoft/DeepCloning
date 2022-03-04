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
        internal protected delegate object RedirectDelegate(object src, DeepCloningOptions options);
        internal protected delegate T CloneOneDimArrayDelegate<T>(T src, int length, Dictionary<object, object> objs, DeepCloningOptions options);
        internal protected delegate T CloneMultiDimArrayDelegate<T>(T src, int[] length, Dictionary<object, object> objs, DeepCloningOptions options);

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

        internal static bool IsSimpleType(Type type)
        {
            if (type.IsPrimitive || type.IsEnum || type == typeof(DateTime) || type == typeof(decimal) || type == typeof(Guid))
            {
                return true;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var t = type.GetGenericArguments()[0];
                return (t.IsPrimitive || t.IsEnum || t == typeof(DateTime) || t == typeof(decimal) || t == typeof(Guid));
            }

            return false;
        }

        internal protected static RedirectDelegate BuildRedirect(Type type)
        {
            var method = new DynamicMethod(String.Empty, typeof(object), new Type[] { typeof(object), typeof(DeepCloningOptions) });
            var il = method.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);

            if (type.IsValueType)
            {
                if (IsSimpleType(type))
                {
                    il.Emit(OpCodes.Ret);
                }

                il.Emit(OpCodes.Unbox_Any, type);
            }

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, typeof(DeepCloning<>).MakeGenericType(type).GetMethod(nameof(DeepCloning<object>.Copy), new[] { type, typeof(DeepCloningOptions) }));

            if (type.IsValueType)
            {
                il.Emit(OpCodes.Box, type);
            }

            il.Emit(OpCodes.Ret);

#if NET5_0_OR_GREATER
            return method.CreateDelegate<RedirectDelegate>();
#else
            return (RedirectDelegate)method.CreateDelegate(typeof(RedirectDelegate));            
#endif
        }
    }
}

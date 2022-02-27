using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ShereSoft.SpecializedCloners
{
    static class TupleCloner
    {
        public static bool CanMap(object value)
        {
            var type = value.GetType();

            return (type.IsGenericType && (
                type.GetGenericTypeDefinition() == typeof(Tuple<>)
                || type.GetGenericTypeDefinition() == typeof(Tuple<,>)
                || type.GetGenericTypeDefinition() == typeof(Tuple<,,>)
                || type.GetGenericTypeDefinition() == typeof(Tuple<,,,>)
                || type.GetGenericTypeDefinition() == typeof(Tuple<,,,,>)
                || type.GetGenericTypeDefinition() == typeof(Tuple<,,,,,>)
                || type.GetGenericTypeDefinition() == typeof(Tuple<,,,,,,>)
                || type.GetGenericTypeDefinition() == typeof(Tuple<,,,,,,,>)
                ));
        }

        public static CloneObjectDelegate<T> Buid<T>()
        {
            var type = typeof(T);
            var method = new DynamicMethod(String.Empty, type, new Type[] { type, typeof(Dictionary<object, object>), typeof(DeepCloningOptions) });
            var il = method.GetILGenerator();

            il.DeclareLocal(typeof(bool));  // DeepCloneStrings

            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Call, typeof(DeepCloningOptions).GetProperty(nameof(DeepCloningOptions.None.DeepCloneStrings)).GetGetMethod());
            il.Emit(OpCodes.Stloc_0);

            foreach (var prop in type.GetProperties().OrderBy(p => p.Name))
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, prop.GetGetMethod());

                if (prop.PropertyType.IsValueType)
                {
                    if (!DeepCloning.IsSimpleType(prop.PropertyType))
                    {
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Ldarg_2);
                        il.Emit(OpCodes.Call, typeof(DeepCloning<>).MakeGenericType(prop.PropertyType).GetMethod(nameof(DeepCloning<T>.DeepCloneStruct), BindingFlags.NonPublic | BindingFlags.Static));
                    }
                }
                else if (prop.PropertyType == typeof(string))
                {
                    il.Emit(OpCodes.Ldloc_0);
                    var skipDeepCloneString = il.DefineLabel();
                    il.Emit(OpCodes.Brfalse, skipDeepCloneString);
                    il.Emit(OpCodes.Callvirt, typeof(string).GetMethod(nameof(String.Empty.ToCharArray), Type.EmptyTypes));
                    il.Emit(OpCodes.Newobj, typeof(string).GetConstructor(new[] { typeof(char[]) }));
                    il.MarkLabel(skipDeepCloneString);
                }
                else
                {
                    il.Emit(OpCodes.Dup);
                    var lblSkipSetIfNull = il.DefineLabel();
                    il.Emit(OpCodes.Brfalse, lblSkipSetIfNull);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldarg_2);
                    il.Emit(OpCodes.Call, typeof(DeepCloning<>).MakeGenericType(prop.PropertyType).GetMethod(nameof(DeepCloning<T>.DeepCloneObject), BindingFlags.NonPublic | BindingFlags.Static));
                    il.MarkLabel(lblSkipSetIfNull);
                }
            }

            var tupleCreate = typeof(Tuple).GetMethods().First(m => m.Name == "Create" && m.GetGenericArguments().Length == type.GetGenericArguments().Length).MakeGenericMethod(type.GetGenericArguments());
            il.Emit(OpCodes.Call, tupleCreate);
            il.Emit(OpCodes.Ret);

#if NET5_0_OR_GREATER
            return method.CreateDelegate<CloneObjectDelegate<T>>();
#else
            return (CloneObjectDelegate<T>)method.CreateDelegate(typeof(CloneObjectDelegate<T>));
#endif
        }
    }
}

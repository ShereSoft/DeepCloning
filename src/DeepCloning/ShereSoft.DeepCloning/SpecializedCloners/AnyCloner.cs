using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ShereSoft.SpecializedCloners
{
    static class AnyCloner
    {
        public static CloneObjectDelegate<T> Buid<T>()
        {
            var type = typeof(T);
            var method = new DynamicMethod(String.Empty, type, new Type[] { type, typeof(Dictionary<object, object>), typeof(DeepCloningOptions) });
            var il = method.GetILGenerator();

            if (type == typeof(Type))
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ret);

#if NET45 || NETCOREAPP
                return (CloneObjectDelegate<T>)method.CreateDelegate(typeof(CloneObjectDelegate<T>));
#else
                return method.CreateDelegate<CloneObjectDelegate<T>>();
#endif
            }

            il.DeclareLocal(type);
            il.DeclareLocal(typeof(bool));  // DeepCloneStrings

            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Call, typeof(DeepCloningOptions).GetProperty(nameof(DeepCloningOptions.None.DeepCloneSingletons)).GetMethod);

            var skipSingleTonDeepCloning = il.DefineLabel();
            il.Emit(OpCodes.Brtrue, skipSingleTonDeepCloning);

            if (!type.IsValueType)
            {
                foreach (var singleton in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).Where(f => f.FieldType == type && f.IsInitOnly))
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldsfld, singleton);
                    var skipSingleTon = il.DefineLabel();
                    il.Emit(OpCodes.Bne_Un, skipSingleTon);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ret);
                    il.MarkLabel(skipSingleTon);
                }
            }

            il.MarkLabel(skipSingleTonDeepCloning);

            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Call, typeof(DeepCloningOptions).GetProperty(nameof(DeepCloningOptions.None.DeepCloneStrings)).GetMethod);
            il.Emit(OpCodes.Stloc_1);

            var target = type.GetConstructor(Type.EmptyTypes);

            if (target != null)
            {
                il.Emit(OpCodes.Newobj, target);
                il.Emit(OpCodes.Stloc_0);
            }
            else if (type.IsValueType)
            {
                il.Emit(OpCodes.Ldloca_S, 0);
                il.Emit(OpCodes.Initobj, type);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, type.GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance));
                il.Emit(OpCodes.Castclass, type);
                il.Emit(OpCodes.Stloc_0);
            }

            if (!type.IsValueType)
            {
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Callvirt, typeof(Dictionary<object, object>).GetMethod("Add"));
            }

            var baseType = type;

            while (baseType != typeof(object))
            {
                foreach (var field in baseType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    var ft = field.FieldType;

                    if (type.IsValueType)
                    {
                        il.Emit(OpCodes.Ldloca_S, 0);
                        il.Emit(OpCodes.Ldarga_S, 0);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ldarg_0);
                    }

                    il.Emit(OpCodes.Ldfld, field);

                    if (ft.IsValueType)
                    {
                        if (!DeepCloning.IsSimpleType(ft))  // inline optimization
                        {
                            il.Emit(OpCodes.Ldarg_1);
                            il.Emit(OpCodes.Ldarg_2);
                            il.Emit(OpCodes.Call, typeof(DeepCloning<>).MakeGenericType(ft).GetMethod(nameof(DeepCloning<T>.DeepCloneStruct), BindingFlags.NonPublic | BindingFlags.Static));
                        }

                        il.Emit(OpCodes.Stfld, field);
                    }
                    else if (ft == typeof(string))  // inline optimization
                    {
                        il.Emit(OpCodes.Ldloc_1);
                        var skipDeepCloneString = il.DefineLabel();
                        il.Emit(OpCodes.Brfalse, skipDeepCloneString);
                        il.Emit(OpCodes.Callvirt, typeof(string).GetMethod(nameof(String.Empty.ToCharArray), Type.EmptyTypes));
                        il.Emit(OpCodes.Newobj, typeof(string).GetConstructor(new[] { typeof(char[]) }));
                        il.MarkLabel(skipDeepCloneString);
                        il.Emit(OpCodes.Stfld, field);
                    }
                    else if (ft == typeof(Type))
                    {
                        il.Emit(OpCodes.Stfld, field);
                    }
                    else
                    {
                        il.Emit(OpCodes.Dup);

                        var lblSkipSetIfNull = il.DefineLabel();
                        il.Emit(OpCodes.Brfalse, lblSkipSetIfNull);

                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Ldarg_2);
                        il.Emit(OpCodes.Call, typeof(DeepCloning<>).MakeGenericType(ft).GetMethod(nameof(DeepCloning<T>.DeepCloneObject), BindingFlags.NonPublic | BindingFlags.Static));
                        il.Emit(OpCodes.Stfld, field);
                        var lblAvoidPopIfNotNull = il.DefineLabel();
                        il.Emit(OpCodes.Br, lblAvoidPopIfNotNull);

                        il.MarkLabel(lblSkipSetIfNull);
                        il.Emit(OpCodes.Pop);  // pop null value
                        il.Emit(OpCodes.Pop);  // pop dest ref

                        il.MarkLabel(lblAvoidPopIfNotNull);
                    }
                }

                baseType = baseType.BaseType;
            }

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

#if NET45 || NETCOREAPP
            return (CloneObjectDelegate<T>)method.CreateDelegate(typeof(CloneObjectDelegate<T>));
#else
            return method.CreateDelegate<CloneObjectDelegate<T>>();
#endif
        }
    }
}

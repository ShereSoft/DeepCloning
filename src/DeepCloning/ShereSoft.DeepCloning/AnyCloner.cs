using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace ShereSoft
{
    class AnyCloner : ClonerBase
    {
        public static CloneObjectDelegate<T> Buid<T>()
        {
            var type = typeof(T);
            var method = new DynamicMethod(String.Empty, type, new Type[] { type, typeof(Dictionary<object, object>), typeof(DeepCloningOptions) });
            var il = method.GetILGenerator();

            il.DeclareLocal(type);  // 0: destination
            il.DeclareLocal(typeof(bool));  // 1: DeepCloningOptions.DeepCloneStrings
            il.DeclareLocal(typeof(object));  // 2: existingClone
            il.DeclareLocal(typeof(Type));  // 3: actualType
            il.DeclareLocal(typeof(CloneObjectDelegate));  // 4: redirect

            if (type.IsClass)
            {
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldloca_S, 2);
                il.Emit(OpCodes.Call, ObjectDictionaryByObjectTryGetValueMethodInfo);
                var cacheNotAvailable = il.DefineLabel();
                il.Emit(OpCodes.Brfalse, cacheNotAvailable);
                il.Emit(OpCodes.Ldloc_2);
                il.Emit(OpCodes.Ret);
                il.MarkLabel(cacheNotAvailable);

                if (!type.IsSealed)
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Call, ObjectGetTypeMethodInfo);
                    il.Emit(OpCodes.Stloc_3);
                    il.Emit(OpCodes.Ldloc_3);
                    il.Emit(OpCodes.Ldsfld, typeof(DeepCloning<>).MakeGenericType(type).GetField("DeclaredType", BindingFlags.NonPublic | BindingFlags.Static));
                    il.Emit(OpCodes.Call, TypeOpInequalityMethodInfo);

                    var typeMatched = il.DefineLabel();
                    il.Emit(OpCodes.Brfalse, typeMatched);

                    il.Emit(OpCodes.Ldsfld, typeof(DeepCloning<>).MakeGenericType(type).GetField("Redirects", BindingFlags.NonPublic | BindingFlags.Static));
                    il.Emit(OpCodes.Ldloc_3);
                    il.Emit(OpCodes.Ldloca_S, 4);
                    il.Emit(OpCodes.Call, TypeConcurrentDictionaryByCloneObjectDelegateTryGetValueMethodInfo);

                    var redirectFound = il.DefineLabel();
                    il.Emit(OpCodes.Brtrue, redirectFound);

                    il.Emit(OpCodes.Ldloc_3);
                    il.Emit(OpCodes.Call, DeepCloningBuildRedirectMethodInfo);
                    il.Emit(OpCodes.Stloc_S, 4);
                    il.Emit(OpCodes.Ldsfld, typeof(DeepCloning<>).MakeGenericType(type).GetField("Redirects", BindingFlags.NonPublic | BindingFlags.Static));
                    il.Emit(OpCodes.Ldloc_3);
                    il.Emit(OpCodes.Ldloc_S, 4);
                    il.Emit(OpCodes.Call, TypeConcurrentDictionaryByCloneObjectDelegateTryAddMethodInfo);
                    il.Emit(OpCodes.Pop);

                    il.MarkLabel(redirectFound);
                    il.Emit(OpCodes.Ldloc_S, 4);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldarg_2);
                    il.Emit(OpCodes.Call, CloneObjectDelegateInvokeMethodInfo);
                    il.Emit(OpCodes.Ret);

                    il.MarkLabel(typeMatched);
                }
            }

            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Call, DeepCloningOptionsGetDeepCloneSingletonsMethodInfo);

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
            il.Emit(OpCodes.Call, DeepCloningOptionsGetDeepCloneStringsMethodInfo);
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
                il.Emit(OpCodes.Stloc_0);
            }

            if (!type.IsValueType)
            {
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Call, ObjectDictionaryByObjectAddMethodInfo);
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
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldloc_0);
                    }

                    var needsCloning = !DeepCloning.IsSimpleValueType(ft) && ft != typeof(string);

                    if (needsCloning)
                    {
                        il.Emit(OpCodes.Ldsfld, typeof(DeepCloning<>).MakeGenericType(ft).GetField(nameof(DeepCloning<object>.CloneObject), BindingFlags.NonPublic | BindingFlags.Static));
                    }

                    if (type.IsValueType)
                    {
                        il.Emit(OpCodes.Ldarga_S, 0);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldarg_0);
                    }

                    il.Emit(OpCodes.Ldfld, field);

                    if (ft.IsValueType)
                    {
                        if (!DeepCloning.IsSimpleValueType(ft))
                        {
                            il.Emit(OpCodes.Ldarg_1);
                            il.Emit(OpCodes.Ldarg_2);
                            il.Emit(OpCodes.Call, typeof(CloneObjectDelegate<>).MakeGenericType(ft).GetMethod("Invoke"));
                        }

                        il.Emit(OpCodes.Stfld, field);
                    }
                    else if (ft == typeof(string))
                    {
                        il.Emit(OpCodes.Ldloc_1);
                        var skipDeepCloneString = il.DefineLabel();
                        il.Emit(OpCodes.Brfalse, skipDeepCloneString);
                        il.Emit(OpCodes.Callvirt, StringToCharArrayMethodInfo);
                        il.Emit(OpCodes.Newobj,  StringCtor);
                        il.MarkLabel(skipDeepCloneString);
                        il.Emit(OpCodes.Stfld, field);
                    }
                    else
                    {
                        il.Emit(OpCodes.Dup);

                        var noSettingIfNull = il.DefineLabel();
                        il.Emit(OpCodes.Brfalse, noSettingIfNull);

                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Ldarg_2);
                        il.Emit(OpCodes.Call, typeof(CloneObjectDelegate<>).MakeGenericType(ft).GetMethod("Invoke"));
                        il.Emit(OpCodes.Stfld, field);
                        var gotoEnd = il.DefineLabel();
                        il.Emit(OpCodes.Br, gotoEnd);

                        il.MarkLabel(noSettingIfNull);
                        il.Emit(OpCodes.Pop);  // null value
                        il.Emit(OpCodes.Pop);  // field ref
                        il.Emit(OpCodes.Pop);  // dest ref

                        il.MarkLabel(gotoEnd);
                    }

#if UNDER_DEVELOPMENT
                    il.MarkLabel(endOfFieldLoop);
#endif
                }

                baseType = baseType.BaseType;
            }

            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

#if NET5_0_OR_GREATER
            return method.CreateDelegate<CloneObjectDelegate<T>>();
#else
            return (CloneObjectDelegate<T>)method.CreateDelegate(typeof(CloneObjectDelegate<T>));
#endif
        }

#if NET45 || NET451_OR_GREATER || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        static T CloneSimpleType<T>(T value, Dictionary<object, object> reusableClones, DeepCloningOptions options)
        {
            return value;
        }
    }
}

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

        internal readonly static HashSet<Type> DefaultUnclonableTypes = new HashSet<Type>(new[]
        {
            typeof(Type),
#if !NET40
            typeof(Type).Assembly.Modules.SelectMany(m => m.GetTypes()).FirstOrDefault(t => t.Name == "RuntimeType"),
            typeof(TypeInfo),
#endif
            typeof(System.Globalization.CultureInfo),
            typeof(System.Globalization.NumberFormatInfo),
            typeof(System.Globalization.DateTimeFormatInfo),
            typeof(MemberInfo),
            typeof(PropertyInfo),
            typeof(MethodInfo),
            typeof(FieldInfo),
            typeof(ConstructorInfo),
        });

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

        internal protected static CloneOneDimArrayDelegate<T> BuidOneDimArrayCloner<T>()
        {
            var type = typeof(T);
            var method = new DynamicMethod(String.Empty, type, new Type[] { type, typeof(int), typeof(Dictionary<object, object>), typeof(DeepCloningOptions) });
            var il = method.GetILGenerator();
            var t = type.GetElementType();
            var lblRepeat = il.DefineLabel();
            var lblEvaluate = il.DefineLabel();

            il.DeclareLocal(type);
            il.DeclareLocal(typeof(int));
            il.DeclareLocal(typeof(bool));  // DeepCloneStrings

            il.Emit(OpCodes.Ldarg_3);
            il.Emit(OpCodes.Call, typeof(DeepCloningOptions).GetProperty(nameof(DeepCloningOptions.None.DeepCloneStrings)).GetGetMethod());
            il.Emit(OpCodes.Stloc_2);

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Newarr, t);
            il.Emit(OpCodes.Stloc_0);

            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Callvirt, typeof(Dictionary<object, object>).GetMethod("Add"));

            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Stloc_1);
            il.Emit(OpCodes.Br_S, lblEvaluate);
            il.MarkLabel(lblRepeat);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc_1);

            if (t == typeof(int))
            {
                il.Emit(OpCodes.Ldelem_I4);
                il.Emit(OpCodes.Stelem_I4);
            }
            else if (t == typeof(double))
            {
                il.Emit(OpCodes.Ldelem_R8);
                il.Emit(OpCodes.Stelem_R8);
            }
            else if (t == typeof(long) || t == typeof(ulong))
            {
                il.Emit(OpCodes.Ldelem_I8);
                il.Emit(OpCodes.Stelem_I8);
            }
            else if (t == typeof(short))
            {
                il.Emit(OpCodes.Ldelem_I2);
                il.Emit(OpCodes.Stelem_I2);
            }
            else if (t == typeof(char) || t == typeof(ushort))
            {
                il.Emit(OpCodes.Ldelem_U2);
                il.Emit(OpCodes.Stelem_I2);
            }
            else if (t == typeof(byte) || t == typeof(bool))
            {
                il.Emit(OpCodes.Ldelem_U1);
                il.Emit(OpCodes.Stelem_I1);
            }
            else if (t == typeof(sbyte))
            {
                il.Emit(OpCodes.Ldelem_I1);
                il.Emit(OpCodes.Stelem_I1);
            }
            else if (t == typeof(uint))
            {
                il.Emit(OpCodes.Ldelem_U4);
                il.Emit(OpCodes.Stelem_I4);
            }
            else if (t == typeof(float))
            {
                il.Emit(OpCodes.Ldelem_R4);
                il.Emit(OpCodes.Stelem_R4);
            }
            else if (t == typeof(IntPtr) || t == typeof(UIntPtr))
            {
                il.Emit(OpCodes.Ldelem_I);
                il.Emit(OpCodes.Stelem_I);
            }
            else if (t.IsValueType)
            {
                il.Emit(OpCodes.Ldelem, t);

                if (!IsSimpleType(t))
                {
                    il.Emit(OpCodes.Ldarg_2);
                    il.Emit(OpCodes.Ldarg_3);
                    il.Emit(OpCodes.Call, typeof(DeepCloning<>).MakeGenericType(t).GetMethod(nameof(DeepCloning<T>.DeepCloneStruct), BindingFlags.NonPublic | BindingFlags.Static));
                }

                il.Emit(OpCodes.Stelem, t);
            }
            else if (t == typeof(string))
            {
                il.Emit(OpCodes.Ldelem, t);

                il.Emit(OpCodes.Ldloc_2);
                var skipDeepCloneString = il.DefineLabel();
                il.Emit(OpCodes.Brfalse, skipDeepCloneString);
                il.Emit(OpCodes.Callvirt, typeof(string).GetMethod(nameof(String.Empty.ToCharArray), Type.EmptyTypes));
                il.Emit(OpCodes.Newobj, typeof(string).GetConstructor(new[] { typeof(char[]) }));
                il.MarkLabel(skipDeepCloneString);

                il.Emit(OpCodes.Stelem, t);
            }
            else
            {
                il.Emit(OpCodes.Ldelem_Ref);
                il.Emit(OpCodes.Dup);
                var lblSkipSetIfNull = il.DefineLabel();
                il.Emit(OpCodes.Brfalse, lblSkipSetIfNull);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Ldarg_3);
                il.Emit(OpCodes.Call, typeof(DeepCloning<>).MakeGenericType(t).GetMethod(nameof(DeepCloning<T>.DeepCloneObject), BindingFlags.NonPublic | BindingFlags.Static));
                il.Emit(OpCodes.Stelem_Ref);

                var lblAvoidPopIfNotNull = il.DefineLabel();
                il.Emit(OpCodes.Br, lblAvoidPopIfNotNull);

                il.MarkLabel(lblSkipSetIfNull);
                il.Emit(OpCodes.Pop);
                il.Emit(OpCodes.Pop);
                il.Emit(OpCodes.Pop);
                il.MarkLabel(lblAvoidPopIfNotNull);
            }

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stloc_1);
            il.MarkLabel(lblEvaluate);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Conv_I4);
            il.Emit(OpCodes.Blt_S, lblRepeat);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

#if NET5_0_OR_GREATER
            return method.CreateDelegate<CloneOneDimArrayDelegate<T>>();
#else
            return (CloneOneDimArrayDelegate<T>)method.CreateDelegate(typeof(CloneOneDimArrayDelegate<T>));
#endif
        }

        internal protected static CloneMultiDimArrayDelegate<T> BuidMultiDimArrayCloner<T>(int arrRank)
        {
            var type = typeof(T);
            var method = new DynamicMethod(String.Empty, type, new Type[] { type, typeof(int[]), typeof(Dictionary<object, object>), typeof(DeepCloningOptions) });
            var il = method.GetILGenerator();
            var t = type.GetElementType();

            il.DeclareLocal(type);  // dest
            il.DeclareLocal(typeof(bool));  // DeepCloneStrings

            il.Emit(OpCodes.Ldarg_3);
            il.Emit(OpCodes.Call, typeof(DeepCloningOptions).GetProperty(nameof(DeepCloningOptions.None.DeepCloneStrings)).GetGetMethod());
            il.Emit(OpCodes.Stloc_1);

            var localCount = 1;

            for (int i = 0; i < arrRank; i++)
            {
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldelem_I4);
            }

            var target = type.GetConstructor(Enumerable.Range(0, arrRank).Select(l => typeof(int)).ToArray());

            il.Emit(OpCodes.Newobj, target);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Callvirt, typeof(Dictionary<object, object>).GetMethod("Add"));

            for (int i = 0; i < arrRank; i++)
            {
                il.DeclareLocal(typeof(int));  // rx

                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldelem_I4);
                il.Emit(OpCodes.Stloc, localCount + i);  // rx = lengths[x]
            }

#if NETCOREAPP
            var labelGroups = new Stack<(Label, Label)>();
#else
            var labelGroups = new Stack<Tuple<Label, Label>>();
#endif

            for (int i = 0; i < arrRank; i++)
            {
                il.DeclareLocal(typeof(int));  // x
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Stloc, localCount + arrRank + i);  // x

                var evaluate = il.DefineLabel();
                il.Emit(OpCodes.Br, evaluate);

                var repeat = il.DefineLabel();
                il.MarkLabel(repeat);

#if NETCOREAPP
                labelGroups.Push((evaluate, repeat));
#else
                labelGroups.Push(Tuple.Create(evaluate, repeat));
#endif
            }

            il.Emit(OpCodes.Ldloc_0);  // dest

            for (int i = 0; i < arrRank; i++)
            {
                var x = localCount + arrRank + i;
                il.Emit(OpCodes.Ldloc, x);
            }

            il.Emit(OpCodes.Ldarg_0);  // src

            for (int i = 0; i < arrRank; i++)
            {
                var x = localCount + arrRank + i;
                il.Emit(OpCodes.Ldloc, x);
            }

            il.Emit(OpCodes.Call, type.GetMethod("Get"));

            if (t.IsValueType)
            {
                if (!DeepCloning.IsSimpleType(t))
                {
                    il.Emit(OpCodes.Ldarg_3);
                    il.Emit(OpCodes.Ldarg_S, 4);
                    il.Emit(OpCodes.Call, typeof(DeepCloning<>).MakeGenericType(t).GetMethod(nameof(DeepCloning<T>.DeepCloneStruct), BindingFlags.NonPublic | BindingFlags.Static));
                }

                il.Emit(OpCodes.Call, type.GetMethod("Set"));
            }
            else if (t == typeof(string))
            {
                il.Emit(OpCodes.Ldloc_1);
                var skipDeepCloneString = il.DefineLabel();
                il.Emit(OpCodes.Brfalse, skipDeepCloneString);
                il.Emit(OpCodes.Callvirt, typeof(string).GetMethod(nameof(String.Empty.ToCharArray), Type.EmptyTypes));
                il.Emit(OpCodes.Newobj, typeof(string).GetConstructor(new[] { typeof(char[]) }));
                il.MarkLabel(skipDeepCloneString);

                il.Emit(OpCodes.Call, type.GetMethod("Set"));
            }
            else
            {
                il.Emit(OpCodes.Dup);
                var lblSkipSetIfNull = il.DefineLabel();
                il.Emit(OpCodes.Brfalse, lblSkipSetIfNull);
                il.Emit(OpCodes.Ldarg_3);
                il.Emit(OpCodes.Ldarg_S, 4);
                il.Emit(OpCodes.Call, typeof(DeepCloning<>).MakeGenericType(t).GetMethod(nameof(DeepCloning<T>.DeepCloneObject), BindingFlags.NonPublic | BindingFlags.Static));
                il.Emit(OpCodes.Call, type.GetMethod("Set"));
                var lblAvoidPopIfNotNull = il.DefineLabel();
                il.Emit(OpCodes.Br, lblAvoidPopIfNotNull);

                il.MarkLabel(lblSkipSetIfNull);
                il.Emit(OpCodes.Pop);  // pop null value
                il.Emit(OpCodes.Pop);  // pop dest ref

                il.MarkLabel(lblAvoidPopIfNotNull);
            }

            for (int i = arrRank - 1; i > -1; i--)
            {
                var x = localCount + arrRank + i;
                var rx = localCount + i;

                il.Emit(OpCodes.Ldloc, x);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Stloc, x);

                var labels = labelGroups.Pop();
                var evaluate = labels.Item1;
                var repeat = labels.Item2;

                il.MarkLabel(evaluate);
                il.Emit(OpCodes.Ldloc, x);
                il.Emit(OpCodes.Ldloc, rx);
                il.Emit(OpCodes.Blt, repeat);
            }

            il.Emit(OpCodes.Ldloc_0);  // dest
            il.Emit(OpCodes.Ret);

#if NET5_0_OR_GREATER
            return method.CreateDelegate<CloneMultiDimArrayDelegate<T>>();
#else
            return (CloneMultiDimArrayDelegate<T>)method.CreateDelegate(typeof(CloneMultiDimArrayDelegate<T>));
#endif
        }
    }
}

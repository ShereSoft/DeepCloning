using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ShereSoft.SpecializedCloners
{
    static class OneDimArrayCloner
    {
        public static CloneObjectDelegate<T> Buid<T>()
        {
            var type = typeof(T);
            var method = new DynamicMethod(String.Empty, type, new Type[] { type, typeof(Dictionary<object, object>), typeof(DeepCloningOptions) });
            var il = method.GetILGenerator();
            var t = type.GetElementType();
            var lblRepeat = il.DefineLabel();
            var lblEvaluate = il.DefineLabel();

            il.DeclareLocal(type);
            il.DeclareLocal(typeof(int));
            il.DeclareLocal(typeof(bool));  // DeepCloneStrings
            il.DeclareLocal(type);  // type-casted src

            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Call, typeof(DeepCloningOptions).GetProperty(nameof(DeepCloningOptions.None.DeepCloneStrings)).GetGetMethod());
            il.Emit(OpCodes.Stloc_2);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, type);
            il.Emit(OpCodes.Dup);  // for later calling Length prop
            il.Emit(OpCodes.Stloc_3);

            il.Emit(OpCodes.Call, type.GetProperty("Length").GetGetMethod());
            il.Emit(OpCodes.Newarr, t);
            il.Emit(OpCodes.Stloc_0);

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldloc_3);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Callvirt, typeof(Dictionary<object, object>).GetMethod("Add"));

            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Stloc_1);
            il.Emit(OpCodes.Br_S, lblEvaluate);
            il.MarkLabel(lblRepeat);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldloc_1);

            if (t == typeof(int))
            {
                il.Emit(OpCodes.Ldloc_3);
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Ldelem_I4);
                il.Emit(OpCodes.Stelem_I4);
            }
            else if (t == typeof(double))
            {
                il.Emit(OpCodes.Ldloc_3);
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Ldelem_R8);
                il.Emit(OpCodes.Stelem_R8);
            }
            else if (t == typeof(long) || t == typeof(ulong))
            {
                il.Emit(OpCodes.Ldloc_3);
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Ldelem_I8);
                il.Emit(OpCodes.Stelem_I8);
            }
            else if (t == typeof(short))
            {
                il.Emit(OpCodes.Ldloc_3);
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Ldelem_I2);
                il.Emit(OpCodes.Stelem_I2);
            }
            else if (t == typeof(char) || t == typeof(ushort))
            {
                il.Emit(OpCodes.Ldloc_3);
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Ldelem_U2);
                il.Emit(OpCodes.Stelem_I2);
            }
            else if (t == typeof(byte) || t == typeof(bool))
            {
                il.Emit(OpCodes.Ldloc_3);
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Ldelem_U1);
                il.Emit(OpCodes.Stelem_I1);
            }
            else if (t == typeof(sbyte))
            {
                il.Emit(OpCodes.Ldloc_3);
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Ldelem_I1);
                il.Emit(OpCodes.Stelem_I1);
            }
            else if (t == typeof(uint))
            {
                il.Emit(OpCodes.Ldloc_3);
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Ldelem_U4);
                il.Emit(OpCodes.Stelem_I4);
            }
            else if (t == typeof(float))
            {
                il.Emit(OpCodes.Ldloc_3);
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Ldelem_R4);
                il.Emit(OpCodes.Stelem_R4);
            }
            else if (t == typeof(IntPtr) || t == typeof(UIntPtr))
            {
                il.Emit(OpCodes.Ldloc_3);
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Ldelem_I);
                il.Emit(OpCodes.Stelem_I);
            }
            else if (t.IsValueType)
            {
                if (!DeepCloning.IsSimpleType(t))
                {
                    il.Emit(OpCodes.Ldsfld, typeof(DeepCloning<>).MakeGenericType(t).GetField(nameof(DeepCloning<T>.CloneObject), BindingFlags.NonPublic | BindingFlags.Static));
                    il.Emit(OpCodes.Ldloc_3);
                    il.Emit(OpCodes.Ldloc_1);
                    il.Emit(OpCodes.Ldelem, t);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldarg_2);
                    il.Emit(OpCodes.Call, typeof(CloneObjectDelegate<>).MakeGenericType(t).GetMethod("Invoke"));
                }
                else
                {
                    il.Emit(OpCodes.Ldloc_3);
                    il.Emit(OpCodes.Ldloc_1);
                    il.Emit(OpCodes.Ldelem, t);
                }

                il.Emit(OpCodes.Stelem, t);
            }
            else if (t == typeof(string))
            {
                il.Emit(OpCodes.Ldloc_3);
                il.Emit(OpCodes.Ldloc_1);
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
                il.Emit(OpCodes.Ldsfld, typeof(DeepCloning<>).MakeGenericType(t).GetField(nameof(DeepCloning<T>.CloneObject), BindingFlags.NonPublic | BindingFlags.Static));
                il.Emit(OpCodes.Ldloc_3);
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Ldelem_Ref);
                il.Emit(OpCodes.Dup);
                var lblSkipSetIfNull = il.DefineLabel();
                il.Emit(OpCodes.Brfalse, lblSkipSetIfNull);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Call, typeof(CloneObjectDelegate<>).MakeGenericType(t).GetMethod("Invoke"));
                il.Emit(OpCodes.Stelem_Ref);

                var lblAvoidPopIfNotNull = il.DefineLabel();
                il.Emit(OpCodes.Br, lblAvoidPopIfNotNull);

                il.MarkLabel(lblSkipSetIfNull);
                il.Emit(OpCodes.Pop);  // value
                il.Emit(OpCodes.Pop);  // field
                il.Emit(OpCodes.Pop);  // dest 
                il.Emit(OpCodes.Pop);  // index
                il.MarkLabel(lblAvoidPopIfNotNull);
            }

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stloc_1);
            il.MarkLabel(lblEvaluate);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldloc_3);
            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Conv_I4);
            il.Emit(OpCodes.Blt_S, lblRepeat);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

#if NET5_0_OR_GREATER
            return method.CreateDelegate<CloneObjectDelegate<T>>();
#else
            return (CloneObjectDelegate<T>)method.CreateDelegate(typeof(CloneObjectDelegate<T>));
#endif
        }
    }
}

using System;
using ShereSoft.SpecializedCloners;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ShereSoft
{
    /// <summary>
    /// Provides a set of methods to deep clone an instance of any object
    /// </summary>
    /// <typeparam name="T">The type of object to clone.</typeparam>
    public sealed class DeepCloning<T> : DeepCloning
    {
        static CloneObjectDelegate<T> CloneObject = null;
        static CloneOneDimArrayDelegate<T> CloneOneDimArray = null;
        static CloneMultiDimArrayDelegate<T> CloneMultiDimArray = null;
        static ConcurrentDictionary<Type, RedirectDelegate> Redirects = new ConcurrentDictionary<Type, RedirectDelegate>();

        readonly static Type DeclaredType = typeof(T);

        static DeepCloning()
        {
            if (DeclaredType.IsArray)
            {
                var arrRank = DeclaredType.GetArrayRank();

                if (arrRank == 1)
                {
                    CloneOneDimArray = BuidOneDimArrayCloner<T>();
                }
                else
                {
                    CloneMultiDimArray = BuidMultiDimArrayCloner<T>(arrRank);
                }
            }
            else if (DeclaredType.IsGenericType && (DeclaredType.GetGenericTypeDefinition() == typeof(List<>) || DeclaredType.GetGenericTypeDefinition() == typeof(HashSet<>)))
            {
                CloneObject = GenericListCloner.Buid<T>();
            }
            else if (DeclaredType.IsGenericType && DeclaredType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                CloneObject = GenericDictionaryCloner.Buid<T>();
            }
            else if (DeclaredType.IsGenericType && (
                DeclaredType.GetGenericTypeDefinition() == typeof(Tuple<>)
                || DeclaredType.GetGenericTypeDefinition() == typeof(Tuple<,>)
                || DeclaredType.GetGenericTypeDefinition() == typeof(Tuple<,,>)
                || DeclaredType.GetGenericTypeDefinition() == typeof(Tuple<,,,>)
                || DeclaredType.GetGenericTypeDefinition() == typeof(Tuple<,,,,>)
                || DeclaredType.GetGenericTypeDefinition() == typeof(Tuple<,,,,,>)
                || DeclaredType.GetGenericTypeDefinition() == typeof(Tuple<,,,,,,>)
                || DeclaredType.GetGenericTypeDefinition() == typeof(Tuple<,,,,,,,>)
                ))
            {
                CloneObject = TupleCloner.Buid<T>();
            }
            else if (DeclaredType.IsValueType)
            {
                if (IsSimpleType(DeclaredType))
                {
                    CloneObject = (src, objs, options) => src;
                }
                else
                {
                    var anyCloner = AnyCloner.Buid<T>();

                    if (anyCloner != null)
                    {
                        CloneObject = anyCloner;
                    }
                }
            }
            else if (DeclaredType == typeof(string))
            {
                CloneObject = (src, objs, options) =>
                {
                    if (options.DeepCloneStrings)
                    {
                        return (T)(object)new String(((string)(object)src).ToCharArray());
                    }

                    return src;
                };
            }
            else if (DeclaredType == typeof(object))
            {
                CloneObject = delegate { return (T)new object(); };
            }
            else if (!DeclaredType.IsAbstract && !DeclaredType.IsInterface)
            {
                var anyCloner = AnyCloner.Buid<T>();
                
                if (anyCloner != null)
                {
                    CloneObject = anyCloner;
                }
            }

#if DEBUG
            if (CloneObject != null || CloneOneDimArray != null || CloneMultiDimArray != null)
            {
                CompiledMapperTypes.TryAdd(DeclaredType.FullName, DeclaredType);
            }
#endif
        }

        /// <summary>
        /// Creates a deep-copied instance of the specified object
        /// </summary>
        /// <param name="value">Any object</param>
        /// <returns>A deep-copied instance of the specified object.</returns>
#if NET45 || NET451_OR_GREATER || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T Copy(T value)
        {
            return Copy(value, DeepCloningOptions.None);
        }

        /// <summary>
        /// Creates a deep-copied instance of the specified object with options
        /// </summary>
        /// <param name="value">Any object</param>
        /// <param name="options">Options to control the cloning behavior</param>
        /// <returns>A deep-copied instance of the specified object.</returns>
        public static T Copy(T value, DeepCloningOptions options)
        {
            if (value == null)
            {
                return default;
            }
            
            return DeepCloneObject(value, new Dictionary<object, object>(), options ?? DeepCloningOptions.None);
        }

        internal static T DeepCloneStruct(T value, Dictionary<object, object> reusableClones, DeepCloningOptions options)
        {
            return CloneObject(value, reusableClones, options);
        }

        internal static T DeepCloneObject(T value, Dictionary<object, object> reusableClones, DeepCloningOptions options)
        {
            if (!DeclaredType.IsSealed)
            {
                var actualType = value.GetType();

                if (actualType != DeclaredType)
                {
                    if (!Redirects.TryGetValue(actualType, out var redirect))
                    {
                        redirect = BuildRedirect(actualType);
                        Redirects.TryAdd(actualType, redirect);
                    }

                    return (T)redirect(value, options);
                }
            }

#if UNDER_DEVELOPMENT
            if (options.UnclonableTypes.Contains(DeclaredType))
            {
                if (options.UseDefaultValueForUnclonableTypes)
                {
                    return default;
                }
                else
                {
                    return value;
                }
            }
#endif

            if (!DeclaredType.IsValueType && reusableClones.TryGetValue(value, out var existingClone))
            {
                return (T)existingClone;
            }

            if (CloneObject != null)
            {
                return CloneObject(value, reusableClones, options);
            }

            var arr = (Array)(object)value;
            var arrRank = arr.Rank;

            if (CloneOneDimArray != null)
            {
                return CloneOneDimArray(value, arr.Length, reusableClones, options);
            }

            var lengths = new int[arrRank];

            for (int i = 0; i < lengths.Length; i++)
            {
                lengths[i] = arr.GetLength(i);
            }

            return CloneMultiDimArray(value, lengths, reusableClones, options);
        }
    }
}
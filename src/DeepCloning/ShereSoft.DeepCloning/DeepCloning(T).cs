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

        /// <summary>
        /// Creates a deep-copied instance of the specified object
        /// </summary>
        /// <param name="value">Any object</param>
        /// <returns>A deep-copied instance of the specified object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            if (CloneObject == null)
            {
                lock (DeclaredType)
                {
                    if (CloneObject == null)
                    {
                        CloneObject = AnyCloner.Buid<T>();
                        CompiledMapperTypes.TryAdd(DeclaredType, 0);
                    }
                }
            }

            return CloneObject(value, reusableClones, options);
        }

        internal static T DeepCloneObject(T value, Dictionary<object, object> reusableClones, DeepCloningOptions options)
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

            if (reusableClones.TryGetValue(value, out var existingClone))
            {
                return (T)existingClone;
            }

            if (CloneObject != null)
            {
                return CloneObject(value, reusableClones, options);
            }

            if (actualType.IsValueType)
            {
                if (IsSimpleType(actualType))
                {
                    return value;
                }
                else
                {
                    return DeepCloneStruct(value, reusableClones, options);
                }
            }

            if (actualType == typeof(string))
            {
                if (options.DeepCloneStrings)
                {
                    return (T)(object)new String(((string)(object)value).ToCharArray());
                }

                return value;
            }

            if (actualType == typeof(object))
            {
                return (T)new object();
            }

            if (value is Array arr)
            {
                var arrRank = arr.Rank;

                if (arrRank == 1)
                {
                    if (CloneOneDimArray == null)
                    {
                        lock (DeclaredType)
                        {
                            if (CloneOneDimArray == null)
                            {
                                CloneOneDimArray = BuidOneDimArrayCloner<T>(arr.Length);
                            }
                        }
                    }

                    return CloneOneDimArray((T)(object)arr, arr.Length, reusableClones, options);
                }

                if (CloneMultiDimArray == null)
                {
                    lock (DeclaredType)
                    {
                        if (CloneMultiDimArray == null)
                        {
                            CloneMultiDimArray = BuidMultiDimArrayCloner<T>(arrRank);
                        }
                    }
                }

                var lengths = new int[arrRank];

                for (int i = 0; i < lengths.Length; i++)
                {
                    lengths[i] = arr.GetLength(i);
                }

                return CloneMultiDimArray((T)(object)arr, lengths, reusableClones, options);
            }

            if (GenericListCloner.CanMap(value))
            {
                lock (DeclaredType)
                {
                    if (CloneObject == null)
                    {
                        CloneObject = GenericListCloner.Buid<T>();
                    }
                }
            }
            else if (GenericDictionaryCloner.CanMap(value))
            {
                lock (DeclaredType)
                {
                    if (CloneObject == null)
                    {
                        CloneObject = GenericDictionaryCloner.Buid<T>();
                    }
                }
            }
            else if (TupleCloner.CanMap(value))
            {
                lock (DeclaredType)
                {
                    if (CloneObject == null)
                    {
                        CloneObject = TupleCloner.Buid<T>();
                    }
                }
            }

            if (CloneObject == null)
            {
                lock (DeclaredType)
                {
                    if (CloneObject == null)
                    {
                        CloneObject = AnyCloner.Buid<T>();
                    }
                }
            }

            CompiledMapperTypes.TryAdd(actualType, 0);

            return CloneObject(value, reusableClones, options);
        }
    }
}
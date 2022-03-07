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
        internal readonly static CloneObjectDelegate<T> CloneObject = null;
        internal readonly static ConcurrentDictionary<Type, CloneObjectDelegate> Redirects = new ConcurrentDictionary<Type, CloneObjectDelegate>();
        internal readonly static Type DeclaredType = typeof(T);

        static DeepCloning()
        {
            if (DeclaredType.IsArray)
            {
                var arrRank = DeclaredType.GetArrayRank();

                if (arrRank == 1)
                {
                    CloneObject = OneDimArrayCloner.Buid<T>();
                }
                else
                {
                    CloneObject = MultiDimArrayCloner.Buid<T>();
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
                if (IsSimpleValueType(DeclaredType))
                {
                    CloneObject = CloneSimpleType;
                }
                else
                {
                    CloneObject = AnyCloner.Buid<T>();
                }
            }
            else if (DeclaredType == typeof(string))
            {
                CloneObject = CloneString;
            }
            else if (DeclaredType == typeof(object))
            {
                CloneObject = CloneObjectType;
            }
            else if (DeclaredType.IsAbstract || DeclaredType.IsInterface)
            {
                CloneObject = CloneAbstractType;
            }
            else
            {
                CloneObject = AnyCloner.Buid<T>();
            }

#if DEBUG
            if (CloneObject != null)
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
            if (value == null)
            {
                return default;
            }

            return CloneObject(value, new Dictionary<object, object>(), DeepCloningOptions.None);
        }

        /// <summary>
        /// Creates a deep-copied instance of the specified object with options
        /// </summary>
        /// <param name="value">Any object</param>
        /// <param name="options">Options to control the cloning behavior</param>
        /// <returns>A deep-copied instance of the specified object.</returns>
#if NET45 || NET451_OR_GREATER || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T Copy(T value, DeepCloningOptions options)
        {
            if (value == null)
            {
                return default;
            }

            if (options == null)
            {
                options = DeepCloningOptions.None;
            }

            var reusableClones = new Dictionary<object, object>();

            if (options.UnclonableObjects.Count > 0)
            {
                foreach (var unclonable in options.UnclonableObjects)
                {
                    reusableClones.Add(unclonable, unclonable);
                }
            }

            return CloneObject(value, reusableClones, options);
        }

#if NET45 || NET451_OR_GREATER || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        static T CloneSimpleType(T value, Dictionary<object, object> reusableClones, DeepCloningOptions options)
        {
            return value;
        }

#if NET45 || NET451_OR_GREATER || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        static T CloneString(T value, Dictionary<object, object> reusableClones, DeepCloningOptions options)
        {
            if (options.DeepCloneStrings)
            {
                return (T)(object)new String(((string)(object)value).ToCharArray());
            }

            return value;
        }

#if NET45 || NET451_OR_GREATER || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        static T CloneObjectType(T value, Dictionary<object, object> reusableClones, DeepCloningOptions options)
        {
            var actualType = value.GetType();

            if (actualType.IsValueType)
            {
                if (DeepCloning.IsSimpleValueType(actualType))
                {
                    return value;
                }
            }
            else if (reusableClones.TryGetValue(value, out var existingClone))
            {
                return (T)existingClone;
            }

            if (actualType != DeclaredType)
            {
                if (!Redirects.TryGetValue(actualType, out var redirect))
                {
                    redirect = BuildRedirect(actualType);
                    Redirects.TryAdd(actualType, redirect);
                }

                return (T)redirect(value, reusableClones, options);
            }

            var plainObject = new object();

            reusableClones.Add(value, plainObject);

            return (T)plainObject;
        }

#if NET45 || NET451_OR_GREATER || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        static T CloneAbstractType(T value, Dictionary<object, object> reusableClones, DeepCloningOptions options)
        {
            if (reusableClones.TryGetValue(value, out var existingClone))
            {
                return (T)existingClone;
            }

            var actualType = value.GetType();

            if (!Redirects.TryGetValue(actualType, out var redirect))
            {
                redirect = BuildRedirect(actualType);
                Redirects.TryAdd(actualType, redirect);
            }

            return (T)redirect(value, reusableClones, options);
        }
    }
}
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
        internal static CloneObjectDelegate<T> CloneObject = null;
        static ConcurrentDictionary<Type, RedirectDelegate> Redirects = new ConcurrentDictionary<Type, RedirectDelegate>();

        readonly static Type DeclaredType = typeof(T);

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
                if (IsSimpleType(DeclaredType))
                {
                    CloneObject = CloneSimpleType;
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
                CloneObject = CloneString;
            }
            else if (DeclaredType == typeof(object))
            {
                CloneObject = CloneObjectType;
            }
            else if (!DeclaredType.IsAbstract && !DeclaredType.IsInterface)
            {
                var anyCloner = AnyCloner.Buid<T>();
                
                if (anyCloner != null)
                {
                    if (DeclaredType.IsSealed)
                    {
                        CloneObject = (src, objs, options) =>
                        {
                            if (objs.TryGetValue(src, out var existingClone))
                            {
                                return (T)existingClone;
                            }

                            return anyCloner(src, objs, options);
                        };
                    }
                    else
                    {
                        CloneObject = (src, objs, options) =>
                        {
                            var actualType = src.GetType();

                            if (actualType != DeclaredType)
                            {
                                if (!Redirects.TryGetValue(actualType, out var redirect))
                                {
                                    redirect = BuildRedirect(actualType);
                                    Redirects.TryAdd(actualType, redirect);
                                }

                                return (T)redirect(src, options);
                            }

                            if (objs.TryGetValue(src, out var existingClone))
                            {
                                return (T)existingClone;
                            }

                            return anyCloner(src, objs, options);
                        };
                    }
                }
            }
            else
            {
                CloneObject = CloneAbstractType;
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
            
            return CloneObject(value, new Dictionary<object, object>(), options ?? DeepCloningOptions.None);
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

            return (T)new object();
        }

#if NET45 || NET451_OR_GREATER || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        static T CloneAbstractType(T value, Dictionary<object, object> reusableClones, DeepCloningOptions options)
        {
            var actualType = value.GetType();

            if (!Redirects.TryGetValue(actualType, out var redirect))
            {
                redirect = BuildRedirect(actualType);
                Redirects.TryAdd(actualType, redirect);
            }

            return (T)redirect(value, options);
        }
    }
}
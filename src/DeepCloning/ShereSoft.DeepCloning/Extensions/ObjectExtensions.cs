using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ShereSoft.Extensions
{
    /// <summary>
    /// Provides a set of convenient methods for deep cloning.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Creates a deep copy of the specified object
        /// </summary>
        /// <typeparam name="T">The type of object to clone.</typeparam>
        /// <param name="value">Any object</param>
        /// <returns>A deep-copied instance of the specified object.</returns>
#if NET45 || NET451_OR_GREATER || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T DeepClone<T>(this T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return DeepCloning<T>.CloneObject(value, new Dictionary<object, object>(), DeepCloningOptions.None);
        }

        /// <summary>
        /// Creates a deep copy of the specified object with options
        /// </summary>
        /// <typeparam name="T">The type of object to clone.</typeparam>
        /// <param name="value">Any object</param>
        /// <param name="options">Options to control the cloning behavior</param>
        /// <returns>A deep-copied instance of the specified object.</returns>
#if NET45 || NET451_OR_GREATER || NETCOREAPP
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T DeepClone<T>(this T value, DeepCloningOptions options)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var reusableClones = new Dictionary<object, object>();

            if (options.UnclonableObjects.Count > 0)
            {
                foreach (var unclonable in options.UnclonableObjects)
                {
                    reusableClones.Add(unclonable, unclonable);
                }
            }

            return DeepCloning<T>.CloneObject(value, reusableClones, options);
        }
    }
}

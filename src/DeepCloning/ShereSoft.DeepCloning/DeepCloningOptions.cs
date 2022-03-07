using System;
using System.Collections.Generic;

namespace ShereSoft
{
    /// <summary>
    /// Provides options to be used with ShereSoft.DeepCloning
    /// </summary>
    public class DeepCloningOptions
    {
        /// <summary>
        /// Represents no options. This is a readonly field.
        /// </summary>
        public static readonly DeepCloningOptions None = new DeepCloningOptions();

        /// <summary>
        /// Gets or sets a value that indicates whether the string value gets shallow-copied or deep-copied. Default is false (shallow-copy).
        /// </summary>
#if NET5_0_OR_GREATER
        public bool DeepCloneStrings { get; init; }
#else
        public bool DeepCloneStrings { get; set; }
#endif

        /// <summary>
        /// Gets or sets a value that indicates whether the singleton object gets shallow-copied or deep-copied. Default is false (shallow-copy).
        /// </summary>
#if NET5_0_OR_GREATER
        public bool DeepCloneSingletons { get; init; }
#else
        public bool DeepCloneSingletons { get; set; }
#endif

        /// <summary>
        /// Gets a list of objects that should not be deep-copied.
        /// </summary>
        public ICollection<object> UnclonableObjects { get; } = new HashSet<object>();

#if UNDER_DEVELOPMENT
        /// <summary>
        /// Gets or sets a value that indicates whether an instance of an unclonable type gets shallow copied for converted to default value. Default is false (shallow copy).
        /// </summary>
#if NET5_0_OR_GREATER
        public bool UseDefaultValueForUnclonableTypes { get; init; }
#else
        public bool UseDefaultValueForUnclonableTypes { get; set; }
#endif

        /// <summary>
        /// Adds or removes unclobable types. 
        /// </summary>
        public ICollection<Type> UnclonableTypes { get; } = new HashSet<Type>(DeepCloning.DefaultUnclonableTypes);
#endif
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Gets or sets a value that indicates whether a reference to the string value gets reused or the value gets deep copied. Default is reference copy.
        /// </summary>
#if NET45 || NETCOREAPP
        public bool DeepCloneStrings { get; set; }
#else
        public bool DeepCloneStrings { get; init; }
#endif

        /// <summary>
        /// Gets or sets a value that indicates whether a reference to the singleton object gets reused or the value gets deep copied. Default is reuse.
        /// </summary>
#if NET45 || NETCOREAPP
        public bool DeepCloneSingletons { get; set; }
#else
        public bool DeepCloneSingletons { get; init; }
#endif
    }
}

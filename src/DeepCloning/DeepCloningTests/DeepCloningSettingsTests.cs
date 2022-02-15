using ShereSoft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DeepCloningTests
{
    public class DeepCloningSettingsTests
    {
        [Fact]
        public void Copy_DoesNotDeepCopyString_ByDefault()
        {
            var original = "TEST";
            var clone = DeepCloning<string>.Copy(original);

            Assert.Equal(original, clone);
            Assert.Same(original, clone);
        }

        [Fact]
        public void Copy_DeepCopiesString_WithSetting()
        {
            var original = "TEST";
            var clone = DeepCloning<string>.Copy(original, new DeepCloningOptions { DeepCloneStrings = true });

            Assert.Equal(original, clone);
            Assert.NotSame(original, clone);
        }

        [Fact]
        public void Copy_ReusesSingleton_ByDefault()
        {
            var original = DeepCloningOptions.None;
            var clone = DeepCloning<DeepCloningOptions>.Copy(original);

            Assert.Same(original, clone);
            Assert.Equal(original.DeepCloneStrings, clone.DeepCloneStrings);
            Assert.Equal(original.DeepCloneSingletons, clone.DeepCloneSingletons);
        }

        [Fact]
        public void Copy_DeepCopiesSingleton_WithOptions()
        {
            var original = DeepCloningOptions.None;
            var clone = DeepCloning<DeepCloningOptions>.Copy(original, new DeepCloningOptions { DeepCloneSingletons = true });

            Assert.NotSame(original, clone);
            Assert.Equal(original.DeepCloneStrings, clone.DeepCloneStrings);
            Assert.Equal(original.DeepCloneSingletons, clone.DeepCloneSingletons);
        }
    }
}

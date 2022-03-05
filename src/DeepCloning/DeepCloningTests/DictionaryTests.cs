using ShereSoft;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Xunit;

namespace DeepCloningTests
{
    public class DictionaryTests
    {
        [Fact]
        public void Copy_DeepCopies_StringDictionaryByInt()
        {
            var original = new Dictionary<int, string> { { 1, "ONE" }, { 2, "TWO" }, { 3, "THREE" }, { 4, "FOUR" }, { 5, "FIVE" } };

            var clone = DeepCloning<Dictionary<int, string>>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original.GetType(), clone.GetType());
            Assert.Equal(original.Count, clone.Count);

            foreach (var key in original.Keys)
            {
                if (original[key] == null)
                {
                    Assert.Null(clone[key]);
                }
                else
                {
                    Assert.Equal(original[key], clone[key]);
                }
            }
        }

        [Fact]
        public void Copy_DeepCopies_IntDictionaryByString()
        {
            var original = new Dictionary<string, int> { { "ONE", 1 }, { "TWO", 2 }, { "THREE", 3 } };

            var clone = DeepCloning<Dictionary<string, int>>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original.GetType(), clone.GetType());
            Assert.Equal(original.Count, clone.Count);

            foreach (var key in original.Keys)
            {
                Assert.Equal(original[key], clone[key]);
            }
        }

        [Fact]
        public void Copy_DeepCopies_IntDictionaryByClass()
        {
            var original = new Dictionary<SimpleClass, int> { { new SimpleClass(), 1 }, { new SimpleClass(), 2 } };

            var clone = DeepCloning<Dictionary<SimpleClass, int>>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original.GetType(), clone.GetType());
            Assert.Equal(original.Count, clone.Count);
        }

        [Fact]
        public void Copy_DeepCopies_ClassDictionaryByInt()
        {
            var original = new Dictionary<int, SimpleClass> { { 1, new SimpleClass() }, { 2, null } };

            var clone = DeepCloning<Dictionary<int, SimpleClass>>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original.GetType(), clone.GetType());
            Assert.Equal(original.Count, clone.Count);

            foreach (var key in original.Keys)
            {
                if (original[key] == null)
                {
                    Assert.Null(clone[key]);
                }
                else
                {
                    Assert.Equal(original[key].GetType(), clone[key].GetType());
                }
            }
        }
    }
}

using ShereSoft;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Xunit;

namespace DeepCloningTests
{
    public class GenericListTests
    {
        [Fact]
        public void Copy_DeepCopies_GenericListOfValueTypes()
        {
            var original = new List<int> { 100, 200, 300, 400, 500 };

            var clone = DeepCloning<List<int>>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original.Count, clone.Count);

            for (int i = 0; i < original.Count; i++)
            {
                Assert.Equal(original[i], clone[i]);
            }
        }

        [Fact]
        public void Copy_DeepCopies_GenericListOfStrings()
        {
            var original = new List<string> { "", "ABC", "XYZ", null };

            var clone = DeepCloning<List<string>>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original.Count, clone.Count);

            for (int i = 0; i < original.Count; i++)
            {
                Assert.Equal(original[i], clone[i]);
            }
        }

        [Fact]
        public void Copy_DeepCopies_ListOfNullableObjects()
        {
            var original = new List<SimpleClass> { new SimpleClass { DateTime = DateTime.UtcNow }, new SimpleClass(), null, };

            var clone = DeepCloning<List<SimpleClass>>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original.GetType(), clone.GetType());
            Assert.Equal(original.Count, clone.Count);

            for (int i = 0; i < original.Count; i++)
            {
                if (original[i] == null)
                {
                    Assert.Null(clone[i]);
                }
                else
                {
                    Assert.Equal(original[i].DateTime, clone[i].DateTime);
                }
            }
        }

        [Fact]
        public void Copy_DeepCopies_ListOfStructs()
        {
            var original = new List<TestStruct> { new TestStruct { String = "C1", Int64 = 147 }, new TestStruct { String = "C2", Int64 = 852 } };

            var clone = DeepCloning<List<TestStruct>>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original.GetType(), clone.GetType());
            Assert.Equal(original.Count, clone.Count);

            for (int i = 0; i < original.Count; i++)
            {
                Assert.Equal(original[i].String, clone[i].String);
                Assert.Equal(original[i].Int64, clone[i].Int64);
            }
        }

        [Fact]
        public void Copy_DeepCopies_GenericIListOfStrings()
        {
            IList<string> original = new List<string> { "", "FIRST", "SECOND", null };

            var clone = DeepCloning<IList<string>>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original.Count, clone.Count);

            for (int i = 0; i < original.Count; i++)
            {
                Assert.Equal(original[i], clone[i]);
            }
        }

        [Fact]
        public void Copy_DeepCopies_HashSetOfStrings()
        {
            var original = new HashSet<string> { "", "ONE", "TWO", null, "FOUR" };

            var clone = DeepCloning<HashSet<string>>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original.Count, clone.Count);
            Assert.True(original.OrderBy(s => s).SequenceEqual(clone.OrderBy(s => s)));
        }
    }
}

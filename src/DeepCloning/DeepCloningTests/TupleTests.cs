using ShereSoft;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Xunit;

namespace DeepCloningTests
{
    public class TupleTests
    {
        [Fact]
        public void Copy_DeepCopies_TupleOfIntAndString()
        {
            var original = Tuple.Create(123, "IJK");

            var clone = DeepCloning<Tuple<int, string>>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original.Item1, clone.Item1);
            Assert.Equal(original.Item2, clone.Item2);
        }

        [Fact]
        public void Copy_DeepCopies_TupleOfVariousTypes()
        {
            var original = Tuple.Create(new SimpleClass(), new TestStruct(), (SimpleClass)null, (string)null);

            var clone = DeepCloning<Tuple<SimpleClass, TestStruct, SimpleClass, string>>.Copy(original);

            Assert.NotSame(original, clone);
            Assert.Same(original.GetType(), clone.GetType());
            Assert.Equal(original.Item1.GetType(), clone.Item1.GetType());
            Assert.Equal(original.Item2.GetType(), clone.Item2.GetType());
            Assert.Equal(original.Item3 == null, clone.Item3 == null);
            Assert.Equal(original.Item4 == null, clone.Item4 == null);
        }
    }
}

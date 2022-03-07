using ShereSoft;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Xunit;

namespace DeepCloningTests
{
    public class ImmutableTests
    {
        [Fact]
        public void Copy_DeepCopies_Immutable()
        {
            var original = new Immutable(333) { Flag = true };

            var clone = (Immutable)DeepCloning<object>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original.Flag, clone.Flag);
            Assert.Equal(original.GetOnly, clone.GetOnly);
            Assert.Equal(original.PrivateSet, clone.PrivateSet);
        }

        [Fact]
        public void Copy_DeepCopies_AnonymousType()
        {
            var original = new
            {
                Int = 123,
                String = "STR",
                SimpleClass = new SimpleClass
                {
                    DateTime = DateTime.UtcNow,
                    Int = 456,
                    String = "TEST",
                    Struct = new TestStruct
                    {
                        Int64 = 789,
                    },
                },
                AnonymousType = new
                {
                    Int = 789,
                },
            };

            dynamic clone = DeepCloning<object>.Copy(original);

            Assert.NotSame(original, clone);
            Assert.Equal(original.Int, clone.Int);
            Assert.Equal(original.String, clone.String);

            Assert.NotSame(original.SimpleClass, clone.SimpleClass);
            Assert.Equal(original.SimpleClass.DateTime, clone.SimpleClass.DateTime);
            Assert.Equal(original.SimpleClass.Int, clone.SimpleClass.Int);
            Assert.Equal(original.SimpleClass.String, clone.SimpleClass.String);
            Assert.Equal(original.SimpleClass.Struct.Int64, clone.SimpleClass.Struct.Int64);
            Assert.Equal(original.AnonymousType.Int, clone.AnonymousType.Int);
        }

        class Immutable
        {
            public int GetOnly { get; }
            public int PrivateSet { get; private set; }
            public bool Flag { get; init; }
            public long ReadOnlyInt64 { get => GetOnly; }

            public Immutable(int value)
            {
                new object();
                GetOnly = value;
                PrivateSet = value;
            }
        }
    }
}

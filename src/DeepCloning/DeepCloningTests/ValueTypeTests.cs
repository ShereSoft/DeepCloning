using ShereSoft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DeepCloningTests
{
    public class ValueTypeTests
    {
        [Fact]
        public void Copy_DeepCopies_Int()
        {
            var original = 12345;
            var clone = DeepCloning<int>.Copy(original);

            Assert.Equal(original, clone);
        }

        [Fact]
        public void Copy_DeepCopies_Guid()
        {
            var original = Guid.NewGuid();
            var clone = DeepCloning<Guid>.Copy(original);

            Assert.Equal(original, clone);
        }

        [Fact]
        public void Copy_DeepCopies_DateTime()
        {
            var original = DateTime.UtcNow;
            var clone = DeepCloning<DateTime>.Copy(original);

            Assert.Equal(original, clone);
        }

        [Fact]
        public void Copy_DeepCopies_NullableStruct()
        {
            decimal? original = 12345.6789m;
            var clone = DeepCloning<decimal?>.Copy(original);

            Assert.Equal(original, clone);
        }

        [Fact]
        public void Copy_DeepCopies_BoxedDouble()
        {
            var original = double.MaxValue;
            var clone = (double)DeepCloning<object>.Copy(original);

            Assert.Equal(original, clone);
        }

        [Fact]
        public void Copy_DeepCopies_BoxedSimpleProperties()
        {
            var original = new SimpleStruct
            {
                Int64 = 987654,
                DateTime = DateTime.UtcNow,
                Decimal = decimal.MaxValue,
                Guid = Guid.NewGuid(),
                String = "STR",
            };

            var boxed = (object)original;

            var clone = DeepCloning<object>.Copy(boxed);

            var unboxedClone = (SimpleStruct)clone;

            Assert.Equal(original.Int64, unboxedClone.Int64);
            Assert.Equal(original.DateTime, unboxedClone.DateTime);
            Assert.Equal(original.Decimal, unboxedClone.Decimal);
            Assert.Equal(original.Guid, unboxedClone.Guid);
            Assert.Equal(original.String, unboxedClone.String);
        }

        [Fact]
        public void Copy_DeepCopies_SimpleProperties()
        {
            var original = new SimpleStruct
            {
                Int64 = 987654,
                DateTime = DateTime.UtcNow,
                Decimal = decimal.MaxValue,
                Guid = Guid.NewGuid(),
                String = "STR",
            };

            var clone = DeepCloning<SimpleStruct>.Copy(original);

            Assert.Equal(original.Int64, clone.Int64);
            Assert.Equal(original.DateTime, clone.DateTime);
            Assert.Equal(original.Decimal, clone.Decimal);
            Assert.Equal(original.Guid, clone.Guid);
            Assert.Equal(original.String, clone.String);
        }

        [Fact]
        public void Copy_DeepCopies_ReadOnlyProperties()
        {
            var original = new ReadOnlySimpleStruct(12345, "STR", Guid.NewGuid());

            var clone = DeepCloning<ReadOnlySimpleStruct>.Copy(original);

            Assert.Equal(original.Index, clone.Index);
            Assert.Equal(original.Name, clone.Name);
            Assert.Equal(original.Id, clone.Id);
            Assert.Equal(original.FormattedName, clone.FormattedName);
        }

        [Fact]
        public void Copy_DeepCopies_PublicField()
        {
            var original = new StructWithPublicField
            {
                Value = "OK"
            };

            var clone = DeepCloning<StructWithPublicField>.Copy(original);

            Assert.Equal(original.Value, clone.Value);
        }

        [Fact]
        public void Copy_DeepCopies_ValueTuple()
        {
            var original = (123, "YKK");

            var clone = DeepCloning<ValueTuple<int, string>>.Copy(original);

            Assert.Equal(original.Item1, clone.Item1);
            Assert.Equal(original.Item2, clone.Item2);
        }
    }

    struct StructWithPublicField
    {
        public string Value;
    }

    struct ReadOnlySimpleStruct
    {
        public int Index { get; }
        public string? Name { get; init; }
        public Guid Id { get; private set; }
        public string? FormattedName => Name != null && Name.Length > 2 ? $"{Char.ToUpper(Name[0])}{Name.Substring(1).ToLower()}" : Name?.ToUpper();

        public ReadOnlySimpleStruct(int index, string name, Guid id)
        {
            Index = index;
            Name = name;
            Id = id;
        }
    }

    struct SimpleStruct
    {
        public long Int64 { get; set; }
        public DateTime DateTime { get; set; }
        public decimal Decimal { get; set; }
        public Guid Guid { get; set; }
        public string String { get; set; }
    }
}

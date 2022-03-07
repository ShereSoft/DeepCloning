using ShereSoft;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace DeepCloningTests
{
    public class SpecialClassTests
    {
        [Fact]
        public void Copy_ShallowCopies_StringsByDefault()
        {
            var original = new SimpleClass { String = "TestClass.String" };
            var clone = DeepCloning<SimpleClass>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotEqual(original, clone);
            Assert.Equal(original.String, clone.String);
            Assert.Same(original.String, clone.String);
        }

        [Fact]
        public void Copy_DeepCopies_JsonElement()
        {
            var json = JsonSerializer.Serialize(new { str = "abcde", idx = 999, vals = new[] { "A", "B" } });
            var original = JsonSerializer.Deserialize<JsonElement>(json);
            var clone = DeepCloning<JsonElement>.Copy(original);

            Assert.True(JsonSerializer.Serialize(original).OrderBy(c => c).SequenceEqual(JsonSerializer.Serialize(clone).OrderBy(c => c)));
        }

        [Fact]
        public void Copy_DeepCopies_ObjectDefinedAsAbstract()
        {
            var original = new TestDerivedClass { Index = 1, Name = "OK" };

            var clone = DeepCloning<TestAbstractClass>.Copy(original);

            Assert.NotSame(original, clone);
            Assert.Equal(original.GetType(), clone.GetType());
            Assert.Equal(original.Index, clone.Index);
            Assert.Equal(original.Name, clone.Name);
        }

        [Fact]
        public void Copy_DeepCopies_ObjectDefinedAsBase()
        {
            var original = new TestMoreDerivedClass { Index = 1, Name = "OK" };

            var clone = DeepCloning<TestDerivedClass>.Copy(original);

            Assert.NotSame(original, clone);
            Assert.Equal(original.GetType(), clone.GetType());
            Assert.Equal(original.Index, clone.Index);
            Assert.Equal(original.Name, clone.Name);
        }

        [Fact]
        public void Copy_DeepCopies_SealedClass()
        {
            var original = new SealedClass { String = "456" };

            var clone = DeepCloning<SealedClass>.Copy(original);

            Assert.NotSame(original, clone);
            Assert.Equal(original.GetType(), clone.GetType());
            Assert.Equal(original.String, clone.String);
        }

        [Fact]
        public void Copy_DeepCopies_ExpandoObjectType()
        {
            dynamic original = new ExpandoObject();
            original.Int = 123;
            original.String = "STR";

            dynamic clone = DeepCloning<object>.Copy(original);

            Assert.NotSame(original, clone);
            Assert.IsType<ExpandoObject>(clone);
            Assert.Equal(original.Int, clone.Int);
            Assert.Equal(original.String, clone.String);
        }

        [Fact]
        public void Copy_DeepCopies_CircularReference()
        {
            var original = new SpecialClass();
            original.Self = original;

            dynamic clone = DeepCloning<SpecialClass>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Same(clone, clone.Self);
        }

        [Fact]
        public void Copy_DeepCopies_DerivedClassDefinedAsBaseClass()
        {
            var original = new SpecialClass();
            original.DerivedClassDefinedAsBaseClass = new TestDerivedClass { Index = 1, Name = "OK" };

            var clone = DeepCloning<SpecialClass>.Copy(original);

            Assert.NotNull(clone.DerivedClassDefinedAsBaseClass);
            Assert.NotSame(original.DerivedClassDefinedAsBaseClass, clone.DerivedClassDefinedAsBaseClass);
            Assert.Equal(original.DerivedClassDefinedAsBaseClass.GetType(), clone.DerivedClassDefinedAsBaseClass.GetType());
            Assert.Equal(original.DerivedClassDefinedAsBaseClass.Index, clone.DerivedClassDefinedAsBaseClass.Index);  // Skelp cannot clone prop defined in base
            Assert.Equal(original.DerivedClassDefinedAsBaseClass.Name, clone.DerivedClassDefinedAsBaseClass.Name);
        }

        [Fact]
        public void Copy_DeepCopies_ClassAsObjectType()
        {
            var original = new SpecialClass();
            original.AnyObject = new DeepCloningOptions { DeepCloneStrings = true };

            var clone = DeepCloning<SpecialClass>.Copy(original);

            Assert.NotSame(original.AnyObject, clone.AnyObject);
            Assert.Same(original.AnyObject.GetType(), clone.AnyObject.GetType());
        }
    }

    class SpecialClass
    {
        public SpecialClass Self { get; set; }
        public TestAbstractClass DerivedClassDefinedAsBaseClass { get; set; }
        public object AnyObject { get; set; }
    }

    class SimpleClass
    {
        public int Int { get; set; }
        public DateTime DateTime { get; set; }
        public string String { get; set; }
        public SimpleClass Class { get; set; }
        public TestStruct Struct { get; set; }
    }

    abstract class TestAbstractClass
    {
        public abstract string Name { get; set; }
        public int Index { get; set; }
    }

    class TestDerivedClass : TestAbstractClass
    {
        public override string Name { get; set; }
    }

    class TestMoreDerivedClass : TestDerivedClass
    {
        public override string Name { get; set; }
    }

    sealed class SealedClass
    {
        public string String { get; set; }
    }
}
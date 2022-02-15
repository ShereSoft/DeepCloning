using ShereSoft;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Xunit;

namespace DeepCloningTests
{
    public class ClassTests
    {



        [Fact]
        public void Copy_DeepCopies_PermanentState()
        {
            var original = new StructWithPermanentState();

            var clone = DeepCloning<StructWithPermanentState>.Copy(original);

            Assert.Equal(original.CreatedTimeLocal, clone.CreatedTimeLocal);
        }

        class StructWithPermanentState
        {
            public DateTime CreatedTimeLocal => _createdUtc.AddHours(-8);

            DateTime _createdUtc = DateTime.UtcNow;
        }


        [Fact]
        public void Copy_DeepCopies_Array()
        {
            var original = new[] { 1, 2, 3, 4, 5 };

            var clone = DeepCloning<int[]>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original.Length, clone.Length);

            for (int i = 0; i < original.Length; i++)
            {
                Assert.Equal(original[i], clone[i]);
            }
        }

        [Fact]
        public void Copy_DeepCopies_TwoDimentionalArray()
        {
            var original = new int[,] { { 11, 12 }, { 21, 22 }, { 31, 32 }, { 41, 42 } };

            var clone = DeepCloning<int[,]>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original[0, 0], clone[0, 0]);
            Assert.Equal(original[0, original.GetLength(1) - 1], clone[0, clone.GetLength(1) - 1]);
            Assert.Equal(original[original.GetLength(0) - 1, 0], clone[clone.GetLength(0) - 1, 0]);
            Assert.Equal(original[original.GetLength(0) - 1, original.GetLength(1) - 1], clone[original.GetLength(0) - 1, original.GetLength(1) - 1]);
        }

        [Fact]
        public void Copy_DeepCopies_TwoDimentionalEmptyArray()
        {
            var original = new int[0, 0];

            var clone = DeepCloning<int[,]>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original.Length, clone.Length);
        }

        [Fact]
        public void Copy_DeepCopies_ThreeDimentionalArray()
        {
            var original = new int[,,] { { { 111 }, { 112 }, { 113 } }, { { 114 }, { 115 }, { 116 } }, { { 117 }, { 118 }, { 119 } } };

            var clone = DeepCloning<int[,,]>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original[0, 0, 0], clone[0, 0, 0]);
            Assert.Equal(original[original.GetLength(0) - 1, original.GetLength(1) - 1, original.GetLength(2) - 1], clone[clone.GetLength(0) - 1, clone.GetLength(1) - 1, clone.GetLength(2) - 1]);
        }

        [Fact]
        public void Copy_DeepCopies_ThreeDimentionalEmptyArray()
        {
            var original = new int[0, 0, 0];

            var clone = DeepCloning<int[,,]>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original.Length, clone.Length);
        }

        [Fact]
        public void Copy_DeepCopies_Tuple()
        {
            var original = Tuple.Create(123, "IJK");

            var clone = DeepCloning<Tuple<int, string>>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original.Item1, clone.Item1);
            Assert.Equal(original.Item2, clone.Item2);
        }

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
            var original = new List<string> { "ABC", "XYZ", };

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
        public void Copy_DeepCopies_GenericIListOfValueTypes()
        {
            IList<string> original = new List<string> { "FIRST", "SECOND" };

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
        public void Copy_DeepCopies_HashSet()
        {
            var original = new HashSet<string> { "ONE", "TWO", "ONE" };

            var clone = DeepCloning<HashSet<string>>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original.Count, clone.Count);
            Assert.True(original.All(o => clone.Contains(o)));
        }

        [Fact]
        public void Copy_DeepCopies_Immutable()
        {
            var original = new Immutable(333) { Flag = true };

            var clone = DeepCloning<Immutable>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original.Flag, clone.Flag);
            Assert.Equal(original.GetOnly, clone.GetOnly);
            Assert.Equal(original.PrivateSet, clone.PrivateSet);
        }

        [Fact]
        public void Copy_DeepCopies_ObjectDefinedAsBase()
        {
            TestBaseClass original = new TestDerivedClass { Index = 1, Name = "OK" };

            var clone = DeepCloning<TestBaseClass>.Copy(original);

            Assert.NotSame(original, clone);
            Assert.IsType<TestDerivedClass>(clone);
            Assert.Equal(original.Index, clone.Index);
            Assert.Equal(original.Name, clone.Name);
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
                        Number = 789,
                    },
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
            Assert.Equal(original.SimpleClass.Struct.Number, clone.SimpleClass.Struct.Number);
        }

        [Fact]
        public void Copy_DeepCopies_CircularReference()
        {
            var original = new TestClass();
            original.Self = original;

            dynamic clone = DeepCloning<TestClass>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Same(clone, clone.Self);
        }

        [Fact]
        public void Copy_DeepCopies_ComplexObject()
        {
            var original = new TestClass();
            var clone = DeepCloning<TestClass>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotEqual(original, clone);

            Assert.Equal(original.String, clone.String);
            Assert.Equal(original.Int, clone.Int);
            Assert.Equal(original.DateTime, clone.DateTime);
            Assert.Equal(original.Decimal, clone.Decimal);
            Assert.Equal(original.Char, clone.Char);
            Assert.Equal(original.Boolean, clone.Boolean);
            Assert.Equal(original.Long, clone.Long);
            Assert.Equal(original.ULong, clone.ULong);
            Assert.Equal(original.Double, clone.Double);
            Assert.Equal(original.Float, clone.Float);
            Assert.Equal(original.SByte, clone.SByte);
            Assert.Equal(original.Short, clone.Short);
            Assert.Equal(original.Byte, clone.Byte);
            Assert.Equal(original.UInt, clone.UInt);
            Assert.Equal(original.Enum, clone.Enum);

            Assert.NotNull(clone.Ints);
            Assert.NotSame(original.Ints, clone.Ints);

            for (int i = 0; i < original.Ints.Length; i++)
            {
                Assert.Equal(original.Ints[i], clone.Ints[i]);
            }

            Assert.NotNull(clone.Strings);
            Assert.NotSame(original.Strings, clone.Strings);

            for (int i = 0; i < original.Strings.Length; i++)
            {
                Assert.Equal(original.Strings[i], clone.Strings[i]);
            }

            Assert.NotNull(clone.IntList);
            Assert.NotSame(original.IntList, clone.IntList);

            for (int i = 0; i < original.IntList.Count; i++)
            {
                Assert.Equal(original.IntList[i], clone.IntList[i]);
            }

            Assert.NotNull(clone.RecordList);
            Assert.NotSame(original.RecordList, clone.RecordList);

            for (int i = 0; i < original.RecordList.Count; i++)
            {
                Assert.Equal(original.RecordList[i].Int32, clone.RecordList[i].Int32);
            }

            Assert.NotNull(clone.StringList);
            Assert.NotSame(original.StringList, clone.StringList);

            for (int i = 0; i < original.StringList.Count; i++)
            {
                Assert.Equal(original.Strings[i], clone.Strings[i]);
            }

            Assert.NotNull(clone.Dictionary);
            Assert.NotSame(original.Dictionary, clone.Dictionary);

            for (int i = 0; i < original.Dictionary.Count; i++)
            {
                Assert.Equal(original.Dictionary.ToArray()[i], clone.Dictionary.ToArray()[i]);
            }

            Assert.NotNull(clone.TwoDimArray);
            Assert.NotSame(original.TwoDimArray, clone.TwoDimArray);
            Assert.Equal(original.TwoDimArray[0, 0], clone.TwoDimArray[0, 0]);
            Assert.Equal(original.TwoDimArray[0, original.TwoDimArray.GetLength(1) - 1], clone.TwoDimArray[0, clone.TwoDimArray.GetLength(1) - 1]);
            Assert.Equal(original.TwoDimArray[original.TwoDimArray.GetLength(0) - 1, 0], clone.TwoDimArray[clone.TwoDimArray.GetLength(0) - 1, 0]);
            Assert.Equal(original.TwoDimArray[original.TwoDimArray.GetLength(0) - 1, original.TwoDimArray.GetLength(1) - 1], clone.TwoDimArray[original.TwoDimArray.GetLength(0) - 1, original.TwoDimArray.GetLength(1) - 1]);

            Assert.NotNull(clone.TwoDimEmptyArray);
            Assert.NotSame(original.TwoDimEmptyArray, clone.TwoDimEmptyArray);

            Assert.NotNull(clone.ThreeDimArray);
            Assert.NotSame(original.ThreeDimArray, clone.ThreeDimArray);
            Assert.Equal(original.ThreeDimArray[0, 0, 0], clone.ThreeDimArray[0, 0, 0]);
            Assert.Equal(original.ThreeDimArray[original.ThreeDimArray.GetLength(0) - 1, original.ThreeDimArray.GetLength(1) - 1, original.ThreeDimArray.GetLength(2) - 1], clone.ThreeDimArray[clone.ThreeDimArray.GetLength(0) - 1, clone.ThreeDimArray.GetLength(1) - 1, clone.ThreeDimArray.GetLength(2) - 1]);

            Assert.NotNull(clone.ThreeDimEmptyArray);
            Assert.NotSame(original.ThreeDimEmptyArray, clone.ThreeDimEmptyArray);

            Assert.Equal(original.Struct.Code, clone.Struct.Code);
            Assert.Equal(original.Struct.Number, clone.Struct.Number);

            Assert.NotSame(original.Tuple, clone.Tuple);
            Assert.Equal(original.Tuple.Item1, clone.Tuple.Item1);
            Assert.Equal(original.Tuple.Item2, clone.Tuple.Item2);

            Assert.Equal(original.ValueTuple.Item1, clone.ValueTuple.Item1);
            Assert.Equal(original.ValueTuple.Item2, clone.ValueTuple.Item2);

            Assert.NotNull(clone.StringList);
            Assert.NotSame(original.StringList, clone.StringList);

            for (int i = 0; i < original.StringList.Count; i++)
            {
                Assert.Equal(original.Strings[i], clone.Strings[i]);
            }

            Assert.NotNull(clone.DerivedClassDefinedAsBaseClass);
            Assert.NotSame(original.DerivedClassDefinedAsBaseClass, clone.DerivedClassDefinedAsBaseClass);
            Assert.Equal(original.DerivedClassDefinedAsBaseClass.GetType(), clone.DerivedClassDefinedAsBaseClass.GetType());
            Assert.Equal(original.DerivedClassDefinedAsBaseClass.Index, clone.DerivedClassDefinedAsBaseClass.Index);  // Skelp cannot clone prop defined in base
            Assert.Equal(original.DerivedClassDefinedAsBaseClass.Name, clone.DerivedClassDefinedAsBaseClass.Name);

            Assert.Equal(original.NullableInt.HasValue, clone.NullableInt.HasValue);
            Assert.Equal(original.NullableInt.GetValueOrDefault(), clone.NullableInt.GetValueOrDefault());

            Assert.NotNull(clone.ImmutableObject);
            Assert.Equal(original.ImmutableObject.Flag, clone.ImmutableObject.Flag);
            Assert.Equal(original.ImmutableObject.GetOnly, clone.ImmutableObject.GetOnly);
            Assert.Equal(original.ImmutableObject.PrivateSet, clone.ImmutableObject.PrivateSet);

        }

        public class TestClass
        {
            public string String { get; set; } = "TestClass.String";
            public int Int { get; set; } = 9;
            public DateTime DateTime { get; set; } = DateTime.Now;
            public double Double { get; set; } = 123.4d;
            public decimal Decimal { get; set; } = 456.78m;
            public bool Boolean { get; set; } = true;
            public StringSplitOptions Enum { get; set; } = StringSplitOptions.RemoveEmptyEntries;
            public long Long { get; set; } = long.MaxValue;
            public char Char { get; set; } = 'A';
            public byte Byte { get; set; } = byte.MaxValue;
            public float Float { get; set; } = float.MaxValue;
            public short Short { get; set; } = short.MaxValue;
            public sbyte SByte { get; set; } = sbyte.MaxValue;
            public uint UInt { get; set; } = uint.MaxValue;
            public ulong ULong { get; set; } = ulong.MaxValue;
            public int[] Ints { get; set; } = new[] { 1, 2, 3, 4, 5 };
            public int[,] TwoDimArray { get; set; } = new int[,] { { 11, 12 }, { 21, 22 }, { 31, 32 }, { 41, 42 } };
            public int[,] TwoDimEmptyArray { get; set; } = new int[0, 0];
            public int[,,] ThreeDimArray { get; set; } = new int[,,] { { { 111 }, { 112 }, { 113 } }, { { 114 }, { 115 }, { 116 } }, { { 117 }, { 118 }, { 119 } } };
            public int[,,] ThreeDimEmptyArray { get; set; } = new int[0, 0, 0];
            public string[] Strings { get; set; } = new[] { "ABC", "XYZ", };
            public TestClass Self { get; set; }
            public TestStruct Struct { get; set; } = new TestStruct { Code = "ABC", Number = 999 };
            public List<int> IntList { get; set; } = new List<int> { 100, 200, 300, 400, 500 };
            public List<string> StringList { get; set; } = new List<string> { "ABC", "XYZ", };
            public List<Record> RecordList { get; set; } = new List<Record> { new Record { Int32 = 1 }, new Record { Int32 = 2 } };
            public Dictionary<int, string> Dictionary { get; set; } = new Dictionary<int, string> { { 1, "ONE" }, { 2, "TWO" }, { 3, "THREE" }, { 4, "FOUR" }, { 5, "FIVE" } };
            public Tuple<int, string> Tuple { get; set; } = System.Tuple.Create(123, "IJK");
            public (int, string) ValueTuple { get; set; } = (123, "YKK");
            public IList<string> StringListInterface { get; set; } = new List<string> { "FIRST", "SECOND" };
            public TestBaseClass DerivedClassDefinedAsBaseClass { get; set; } = new TestDerivedClass { Index = 1, Name = "OK" };
            public int? NullableInt { get; set; } = 1;
            public Immutable ImmutableObject { get; set; } = new Immutable(333) { Flag = true };
            public object AnonymousObject { get; set; } = new
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
                        Number = 789,
                    },
                },
            };
            public object AnyObject { get; set; } = DeepCloningOptions.None;
            public HashSet<string> HashSet = new HashSet<string> { "ONE", "TWO", "ONE" };

            public TestClass()
            {
                Self = this;
            }
        }

        public struct TestStruct
        {
            public int Number { get; set; }
            public string Code { get; set; }
        }

        public record Record
        {
            public int Int32 { get; set; }
        }

        public class SimpleClass
        {
            public int Int { get; set; }
            public DateTime DateTime { get; set; }
            public string? String { get; set; }
            public SimpleClass? Class { get; set; }
            public TestStruct Struct { get; set; }
        }

        public abstract class TestBaseClass
        {
            public abstract string? Name { get; set; }
            public int Index { get; set; }
        }

        public class TestDerivedClass : TestBaseClass
        {
            public override string? Name { get; set; }
        }

        public class Immutable
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
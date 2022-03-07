using ShereSoft;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Xunit;

namespace DeepCloningTests
{
    public class ArrayTests
    {
        [Fact]
        public void Copy_DeepCopies_EmptyArrayOfPremitives()
        {
            var original = new int[0];

            var clone = DeepCloning<int[]>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original.Length, clone.Length);
        }

        [Fact]
        public void Copy_DeepCopies_ArrayOfPremitives()
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
        public void Copy_DeepCopies_ArrayOfStrings()
        {
            var original = new[] { "ABC", null, "XYZ", "" };

            var clone = DeepCloning<string[]>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original.GetType(), clone.GetType());
            Assert.Equal(original.Length, clone.Length);

            for (int i = 0; i < original.Length; i++)
            {
                Assert.Equal(original[i], clone[i]);
            }
        }

        [Fact]
        public void Copy_DeepCopies_ArrayOfStructs()
        {
            var original = new[] { new TestStruct { String = "C1", Int64 = 789 }, new TestStruct { String = "C2", Int64 = 456 } };

            var clone = DeepCloning<TestStruct[]>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original.GetType(), clone.GetType());
            Assert.Equal(original.Length, clone.Length);

            for (int i = 0; i < original.Length; i++)
            {
                Assert.Equal(original[i].String, clone[i].String);
                Assert.Equal(original[i].Int64, clone[i].Int64);
            }
        }

        [Fact]
        public void Copy_DeepCopies_ArrayOfClasses()
        {
            var original = new[] { new SimpleClass { Int = 963, String = "QWE" }, new SimpleClass { Int = 852, String = "RTY" }, null, new SimpleClass() };

            var clone = DeepCloning<SimpleClass[]>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original.GetType(), clone.GetType());
            Assert.Equal(original.Length, clone.Length);

            for (int i = 0; i < original.Length; i++)
            {
                if (original[i] == null)
                {
                    Assert.Null(clone[i]);
                }
                else
                {
                    Assert.Equal(original[i].Int, clone[i].Int);
                    Assert.Equal(original[i].String, clone[i].String);
                }
            }
        }

        [Fact]
        public void Copy_DeepCopies_TwoDimentionalEmptyArrayOfPremitives()
        {
            var original = new int[0, 0];

            var clone = DeepCloning<int[,]>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original.Length, clone.Length);
        }

        [Fact]
        public void Copy_DeepCopies_TwoDimentionalArrayOfPremitives()
        {
            var original = new int[,] { { 11, 12 }, { 21, 22 }, { 31, 32 }, { 41, 42 } };

            var clone = DeepCloning<int[,]>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);

            for (int i = 0; i < original.GetLength(0); i++)
            {
                for (int j = 0; j < original.GetLength(1); j++)
                {
                    Assert.Equal(original[i, j], clone[i, j]);
                }
            }
        }

        [Fact]
        public void Copy_DeepCopies_TwoDimArrayOfStructs()
        {
            var original = new TestStruct[,] { { new TestStruct { Guid = Guid.NewGuid() }, new TestStruct { Guid = Guid.NewGuid() } }, { new TestStruct { Guid = Guid.NewGuid() }, new TestStruct { Guid = Guid.NewGuid() } } };

            var clone = DeepCloning<TestStruct[,]>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original.Length, clone.Length);

            for (int i = 0; i < original.GetLength(0); i++)
            {
                for (int j = 0; j < original.GetLength(1); j++)
                {
                    Assert.Equal(original[i, j].Guid, clone[i, j].Guid);
                }
            }
        }

        [Fact]
        public void Copy_DeepCopies_TwoDimArrayOfClasses()
        {
            var original = new SimpleClass[,] { { new SimpleClass { DateTime = DateTime.UtcNow }, new SimpleClass { DateTime = DateTime.UtcNow } }, { new SimpleClass { DateTime = DateTime.UtcNow }, null } };

            var clone = DeepCloning<SimpleClass[,]>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original.Length, clone.Length);

            for (int i = 0; i < original.GetLength(0); i++)
            {
                for (int j = 0; j < original.GetLength(1); j++)
                {
                    if (original[i, j] == null)
                    {
                        Assert.Null(clone[i, j]);
                    }
                    else
                    {
                        Assert.Equal(original[i, j].DateTime, clone[i, j].DateTime);
                    }
                }
            }
        }

        [Fact]
        public void Copy_DeepCopies_TwoDimArrayOfStrings()
        {
            var original = new string[,] { { "A", "B" }, { "C", null }, { "", String.Empty } };

            var clone = DeepCloning<string[,]>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original.Length, clone.Length);

            for (int i = 0; i < original.GetLength(0); i++)
            {
                for (int j = 0; j < original.GetLength(1); j++)
                {
                    if (original[i, j] == null)
                    {
                        Assert.Null(clone[i, j]);
                    }
                    else
                    {
                        Assert.Equal(original[i, j], clone[i, j]);
                    }
                }
            }
        }

        [Fact]
        public void Copy_DeepCopies_ThreeDimentionalEmptyArrayOfPremitives()
        {
            var original = new int[0, 0, 0];

            var clone = DeepCloning<int[,,]>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);
            Assert.Equal(original.Length, clone.Length);
        }

        [Fact]
        public void Copy_DeepCopies_ThreeDimentionalArrayOfPremitives()
        {
            var original = new int[,,] { { { 111 }, { 112 }, { 113 } }, { { 114 }, { 115 }, { 116 } }, { { 117 }, { 118 }, { 119 } } };

            var clone = DeepCloning<int[,,]>.Copy(original);

            Assert.NotNull(clone);
            Assert.NotSame(original, clone);

            for (int i = 0; i < original.GetLength(0); i++)
            {
                for (int j = 0; j < original.GetLength(1); j++)
                {
                    for (int k = 0; k < original.GetLength(2); k++)
                    {
                        Assert.Equal(original[i, j, k], clone[i, j, k]);
                    }
                }
            }
        }
    }
}

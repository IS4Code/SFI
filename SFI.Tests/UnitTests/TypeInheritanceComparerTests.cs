using IS4.SFI.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace IS4.SFI.Tests
{
    /// <summary>
    /// The tests for <see cref="TypeInheritanceComparer{T}"/>.
    /// </summary>
    [TestClass]
    public class TypeInheritanceComparerTests
    {
        /// <summary>
        /// The tests for <see cref="TypeInheritanceComparer{T}.CompareInner(T, T)"/>.
        /// </summary>
        [TestMethod]
        [DataRow(typeof(ItemBase), typeof(ItemDerived1), -1)]
        [DataRow(typeof(ItemBase), typeof(ItemDerived2), -1)]
        [DataRow(typeof(ItemDerived1), typeof(ItemBase), 1)]
        [DataRow(typeof(ItemDerived2), typeof(ItemBase), 1)]
        [DataRow(typeof(ItemBase), typeof(ItemCommon), -1)]
        [DataRow(typeof(ItemCommon), typeof(ItemBase), 1)]
        public void CompareInnerTests(Type leftType, Type rightType, int expected)
        {
            var comparer = new Implementation();
            var left = Activator.CreateInstance(leftType)!;
            var right = Activator.CreateInstance(rightType)!;
            var result = comparer.Compare(left, right);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// The tests for <see cref="TypeInheritanceComparer{T}.CompareInner(T, T)"/>
        /// where the method throws.
        /// </summary>
        [TestMethod]
        [DataRow(typeof(ItemDerived1), typeof(ItemCommon))]
        [DataRow(typeof(ItemDerived2), typeof(ItemCommon))]
        [DataRow(typeof(ItemCommon), typeof(ItemDerived1))]
        [DataRow(typeof(ItemCommon), typeof(ItemDerived2))]
        [DataRow(typeof(ItemCommon), typeof(ItemCommon))]
        [DataRow(typeof(ItemCommon), typeof(ItemCommon))]
        public void CompareInnerExceptionTests(Type leftType, Type rightType)
        {
            var comparer = new Implementation();
            var left = Activator.CreateInstance(leftType)!;
            var right = Activator.CreateInstance(rightType)!;
            Assert.ThrowsException<NotSupportedException>(() => comparer.Compare(left, right));
        }

        class Implementation : TypeInheritanceComparer<object>
        {
            protected override IEnumerable<Type> SelectType(Type initial)
            {
                return initial.GetInterfaces();
            }
        }

        class ItemBase : IBase
        {

        }

        class ItemDerived1 : IDerived1
        {

        }

        class ItemDerived2 : IDerived2
        {

        }

        class ItemCommon : ICommon
        {

        }

        interface IBase
        {

        }

        interface IDerived1 : IBase
        {

        }

        interface IDerived2 : IBase
        {

        }

        interface ICommon : IDerived1, IDerived2
        {

        }
    }
}

using NUnit.Framework;
using System.Linq;

namespace Julians.OverlappingIntervalTree.Tests
{
    /// <summary>
    /// Uses LINQ and Value-comparison to verify internal structure on when adding/removing nodes
    /// </summary>
    [TestFixture]
    internal class TreeMaintenanceTests
    {
        //Note: Test relies on Equals override on Period to identify internal nodes
        Period a = new Period(new Date(2017, 1, 3), new Date(2017, 1, 5));
        Period b = new Period(new Date(2017, 1, 7), new Date(2017, 1, 12));
        Period c = new Period(new Date(2017, 1, 10), new Date(2017, 1, 11));
        Period d = new Period(new Date(2017, 1, 11), new Date(2017, 2, 1));
        Period e = new Period(new Date(2017, 1, 10), new Date(2017, 1, 11)); //e is equal to c

        [Test]
        public void MaintainsInternalIndex_A_RootNode()
        {            
            var sut = new IntervalTree<Date, Period ,char>();

            sut.Add(a, 'a');
            Assert.AreEqual(1, sut.CountNodes());
            CollectionAssert.AreEquivalent("a", sut.RootNode.Values);
        }

        [Test]
        public void MaintainsInternalIndex_AB_SiblingsWithNewRoot()
        {
            var sut = new IntervalTree<Date, Period, char>();

            sut.Add(a, 'a');
            sut.Add(b, 'b');
            Assert.AreEqual(3, sut.CountNodes());
            CollectionAssert.IsEmpty(sut.RootNode.Values);
            var aNode = sut.RootNode.ChildNodes.Single(n => n.Interval.Equals(a));
            var bNode = sut.RootNode.ChildNodes.Single(n => n.Interval.Equals(b));
            CollectionAssert.AreEquivalent("a", aNode.Values);
            CollectionAssert.AreEquivalent("b", bNode.Values);
        }

        [Test]
        public void MaintainsInternalIndex_ABC_ChildAddedToEnclosingNode()
        {
            var sut = new IntervalTree<Date, Period, char>();

            sut.Add(a, 'a');
            sut.Add(b, 'b');
            sut.Add(c, 'c');
            Assert.AreEqual(4, sut.CountNodes());
            CollectionAssert.IsEmpty(sut.RootNode.Values);
            var aNode = sut.RootNode.ChildNodes.Single(n => n.Interval.Equals(a));
            var bNode = sut.RootNode.ChildNodes.Single(n => n.Interval.Equals(b));
            var cNode = bNode.ChildNodes.Single(n => n.Interval.Equals(c));
            CollectionAssert.AreEquivalent("a", aNode.Values);
            CollectionAssert.AreEquivalent("b", bNode.Values);
            CollectionAssert.AreEquivalent("c", cNode.Values);
        }

        [Test]
        public void MaintainsInternalIndex_ABCD_RearrangingWithNewScopes()
        {
            var sut = new IntervalTree<Date, Period, char>();

            sut.Add(a, 'a');
            sut.Add(b, 'b');
            sut.Add(c, 'c');
            sut.Add(d, 'd');
            
            var dNode = sut.RootNode.ChildNodes.Single(n => n.Interval.Equals(d));
            var abParentNode = sut.RootNode.ChildNodes.Single(n => !dNode.Equals(n));
            var aNode = abParentNode.ChildNodes.Single(n => n.Interval.Equals(a));
            var bNode = abParentNode.ChildNodes.Single(n => n.Interval.Equals(b));
            var cNode = bNode.ChildNodes.Single();

            //2 nodes in index does not have values
            Assert.AreEqual(6, sut.CountNodes());
            CollectionAssert.IsEmpty(sut.RootNode.Values);
            CollectionAssert.IsEmpty(abParentNode.Values);
            CollectionAssert.AreEquivalent("a", aNode.Values);
            CollectionAssert.AreEquivalent("b", bNode.Values);
            CollectionAssert.AreEquivalent("c", cNode.Values);
            CollectionAssert.AreEquivalent("d", dNode.Values);
        }

        [Test]
        public void MaintainsInternalIndex_ABCDE_ValuesAreConcatinatedWhenIntervalAlreadyExists()
        {
            var sut = new IntervalTree<Date, Period, char>();

            sut.Add(a, 'a');
            sut.Add(b, 'b');
            sut.Add(c, 'c');
            sut.Add(d, 'd');
            sut.Add(e, 'e'); //period e is same as period c!

            var dNode = sut.RootNode.ChildNodes.Single(n => n.Interval.Equals(d));
            var abParentNode = sut.RootNode.ChildNodes.Single(n => !dNode.Equals(n));
            var bNode = abParentNode.ChildNodes.Single(n => n.Interval.Equals(b));
            var cNode = bNode.ChildNodes.Single();

            CollectionAssert.AreEquivalent("ce", cNode.Values);
        }

        [Test]
        public void MaintainsInternalIndex_ABCDE_RemovingC()
        {
            var sut = new IntervalTree<Date, Period, char>();

            sut.Add(a, 'a');
            sut.Add(b, 'b');
            sut.Add(c, 'c');
            sut.Add(d, 'd');
            sut.Add(e, 'e'); //period e is same as period c!

            sut.RemoveValueAt(e, 'c');
            
            var dNode = sut.RootNode.ChildNodes.Single(n => n.Interval.Equals(d));
            var abParentNode = sut.RootNode.ChildNodes.Single(n => !dNode.Equals(n));
            var bNode = abParentNode.ChildNodes.Single(n => n.Interval.Equals(b));
            var cNode = bNode.ChildNodes.Single();
            
            CollectionAssert.AreEquivalent("e", cNode.Values);
        }

        [Test]
        public void MaintainsInternalIndex_ABCDE_RemovingNothing()
        {
            var sut = new IntervalTree<Date, Period, char>();
            sut.Add(a, 'a');
            sut.Add(b, 'b');
            sut.Add(c, 'c');
            sut.Add(d, 'd');
            sut.Add(e, 'e'); //period e is same as period c!
            var wrongInterval = new Period(new Date(2018, 1, 10), new Date(2018, 1, 11));
            sut.RemoveValueAt(wrongInterval, 'c');
            
            var dNode = sut.RootNode.ChildNodes.Single(n => n.Interval.Equals(d));
            var abParentNode = sut.RootNode.ChildNodes.Single(n => !dNode.Equals(n));
            var bNode = abParentNode.ChildNodes.Single(n => n.Interval.Equals(b));
            var cNode = bNode.ChildNodes.Single();

            CollectionAssert.AreEquivalent("ce", cNode.Values);
        }
    }
}

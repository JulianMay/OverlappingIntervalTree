using NUnit.Framework;
using System.Linq;

namespace Julians.OverlappingIntervalTree.Tests
{
    [TestFixture]
    class TreeLookupTests
    {        
        [Test]
        public void GetsIntersectedValuesCorrectly()
        {
            var sut = new IntervalTree<Date, Period, char>();
            var toIntersect = new Period(new Date(2019, 4, 28), new Date(2019, 5, 4));

            //expected has number-chars as values, unexpected has letter-chars as values
            sut.Add(new Period(new Date(2019, 4, 28), new Date(2019, 5, 4)), '1');
            sut.Add(new Period(new Date(2019, 4, 26), new Date(2019, 4, 27)), 'a');
            sut.Add(new Period(new Date(2019, 4, 30), new Date(2019, 5, 1)), '2');
            sut.Add(new Period(new Date(2019, 5, 5), new Date(2030, 1, 1)), 'b');
            sut.Add(new Period(new Date(2019, 5, 4), new Date(2019, 5, 10)), '3');
            sut.Add(new Period(new Date(2010, 1, 1), new Date(2019, 4, 28)), '4');
            sut.Add(new Period(new Date(2025, 4, 5), new Date(2026, 1, 1)), 'c');
            sut.Add(new Period(new Date(2010, 1, 1), new Date(2040, 12, 31)), '5');

            var result = sut.GetIntersectedValues(toIntersect);

            CollectionAssert.AreEquivalent("12345", result.ToArray());
        }

        [Test]
        public void GetsBoundedValuesCorrectly()
        {
            var sut = new IntervalTree<Date, Period, char>();
            var boundedPeriod = new Period(new Date(2019, 4, 28), new Date(2019, 5, 4));
            //Only '1' and '2' is within bounded period
            sut.Add(new Period(new Date(2019, 4, 28), new Date(2019, 5, 4)), '1');
            sut.Add(new Period(new Date(2019, 4, 26), new Date(2019, 4, 27)), 'a');
            sut.Add(new Period(new Date(2019, 4, 30), new Date(2019, 5, 1)), '2');
            sut.Add(new Period(new Date(2019, 5, 5), new Date(2030, 1, 1)), 'b');
            sut.Add(new Period(new Date(2019, 5, 4), new Date(2019, 5, 10)), '3');
            sut.Add(new Period(new Date(2010, 1, 1), new Date(2019, 4, 28)), '4');
            sut.Add(new Period(new Date(2025, 4, 5), new Date(2026, 1, 1)), 'c');
            sut.Add(new Period(new Date(2010, 1, 1), new Date(2040, 12, 31)), '5');

            var result = sut.GetBoundedValues(boundedPeriod);

            CollectionAssert.AreEquivalent("12", result.ToArray());
        }
    }
}

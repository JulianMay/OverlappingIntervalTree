using System;

namespace Julians.OverlappingIntervalTree
{
    /// <summary>
    /// Value-object representing a date-period
    /// </summary>
    public class Period : IInterval<Date>
    {
        public Date From { get; private set; }
        public Date To { get; private set; }

        public Period(Date from, Date to)
        {
            if (from == null) throw new ArgumentNullException(nameof(from));
            if (to == null) throw new ArgumentNullException(nameof(to));
            if (from.CompareTo(to) > 0) throw new ArgumentException(string.Format($"'{nameof(from)}' ({from} must be earlier than '{nameof(to)} ({to})"));

            From = from;
            To = to;
        }

        public override string ToString()
        {
            return $"from {From} to {To}";
        }

        private Date EarliestBeginning(IInterval<Date> other)
        {
            if (this.From.CompareTo(other.From) < 0)// < other.From)
                return this.From;
            return other.From;
        }

        private Date LatestEnd(IInterval<Date> other)
        {
            if (this.To.CompareTo(other.To) > 0)// > other.To)
                return this.To;
            return other.To;
        }

        public IInterval<Date> Compound(IInterval<Date> other)
        {
            return new Period(EarliestBeginning(other), LatestEnd(other));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + From.GetHashCode();
                hash = hash * 23 + To.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as Period;
            return other != null &&
                other.From.Equals(this.From) &&
                other.To.Equals(this.To);
        }
    }
}

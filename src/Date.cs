using System;

namespace Julians.OverlappingIntervalTree
{
    /// <summary>
    /// Value-object representing a date
    /// </summary>
    public class Date : IComparable<Date>
    {
        public readonly int Year;
        public readonly int Month;
        public readonly int Day;
        public readonly int _value;


        public Date(int year, int month, int day)
        {
            if (year > 3000 || year < 1900) throw new ArgumentException($"Year must be min. 1900 and max. 3000, was {year}");
            if (month > 12 || month < 1) throw new ArgumentException($"Year must be min. 1 and max. 12, was {month}");
            if (day > 31 || day < 1) throw new ArgumentException($"Year must be min. 1 and max. 31, was {day}");

            Year = year;
            Month = month;
            Day = day;
            _value = (Year * 10000) + (Month * 100) + day;
        }

        public override string ToString()
        {
            return $"{Year.ToString("D4")}-{Month.ToString("D2")}-{Day.ToString("D2")}";
        }

        public override bool Equals(object obj)
        {
            var other = obj as Date;
            return other != null && other._value == this._value;
        }

        public override int GetHashCode()
        {
            return _value;
        }

        public int CompareTo(Date other)
        {
            return _value.CompareTo(other._value);
        }
    }
}

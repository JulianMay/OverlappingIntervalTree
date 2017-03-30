using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Julians.OverlappingIntervalTree
{
    public interface IInterval<TBoundery> where TBoundery : IComparable<TBoundery>
    {
        TBoundery From { get; }
        TBoundery To { get; }
        IInterval<TBoundery> Compound(IInterval<TBoundery> other);
    }

    
    public class IntervalTree<TBoundery, TInterval, TValue> where TInterval : IInterval<TBoundery> where TBoundery : IComparable<TBoundery>
    {
        internal IntervalNode<TInterval, TBoundery, TValue> RootNode;

        public void Add(TInterval interval, params TValue[] values)
        {
            var node = IntervalNode<TInterval, TBoundery, TValue>.NewNodeWithChildren(interval, values);

            if (RootNode.IsDefined)
                RootNode = RootNode.WithNode(node);                             
            else
                RootNode = node;
        }

        public void RemoveValueAt(TInterval interval, TValue valueToRemove)
        {
            if (!RootNode.IsDefined)
                return;

            RootNode = RootNode.WithValueRemovedAt(interval, valueToRemove);
        }

        public int CountNodes()
        {
            if (!RootNode.IsDefined)
                return 0;
            return RootNode.Count;
        }

        /// <summary>
        /// Paste output into http://www.webgraphviz.com/
        /// </summary>
        /// <returns></returns>
        public string GraphWizDOT
        {
            get
            {
                {
                    if (!RootNode.IsDefined)
                        return string.Empty;

                    var b = new StringBuilder();
                    b.AppendLine("digraph g{");
                    AppendDOTforNode(b, RootNode);
                    b.AppendLine("}");
                    return b.ToString();
                }
            }
        }

        private void AppendDOTforNode(StringBuilder b, IntervalNode<TInterval,TBoundery,TValue> node)
        {
            foreach (var c in node.ChildNodes)
            {
                b.AppendLine($"\"{node.Interval}\" -> \"{c.Interval}\"");
                AppendDOTforNode(b, c);
            }

            foreach (var v in node.Values)
            {
                b.AppendLine($"\"{node.Interval}\" -> \"{v}\" [color=blue]");
            }
        }

        public IEnumerable<TValue> GetIntersectedValues(TInterval interval)
        {
            if (!RootNode.IsDefined)
                return Enumerable.Empty<TValue>();
            return GetIntersectedValuesPerNode(RootNode, interval);
        }   
        
        private IEnumerable<TValue> GetIntersectedValuesPerNode(IntervalNode<TInterval,TBoundery,TValue> node, TInterval intervalToIntersect)
        {
            var @case = IntervalClassification.Evaluate<TInterval, TBoundery>(node.Interval, intervalToIntersect);
            if (@case == IntervalCase.Unconnected)
                yield break;

            foreach (var v in node.Values)
                yield return v;

            foreach (var cv in node.ChildNodes.SelectMany(c => GetIntersectedValuesPerNode(c, intervalToIntersect)))
                yield return cv;
            
        }
        public IEnumerable<TValue> GetBoundedValues(TInterval interval)
        {
            if (!RootNode.IsDefined)
                return Enumerable.Empty<TValue>();
            return GetBoundedValuesPerNode(RootNode, interval);
        }

        private IEnumerable<TValue> GetBoundedValuesPerNode(IntervalNode<TInterval, TBoundery, TValue> node, TInterval bounds)
        {
            var @case = IntervalClassification.Evaluate<TInterval, TBoundery>(node.Interval, bounds);
            if (@case == IntervalCase.Unconnected)
                yield break;

            if (@case == IntervalCase.AinB || @case == IntervalCase.Same)
            {
                foreach (var v in node.Values)
                    yield return v;
            }

            foreach (var cv in node.ChildNodes.SelectMany(c => GetBoundedValuesPerNode(c, bounds)))
                yield return cv;
        }
    }

    internal struct IntervalNode<TInterval, TBoundery, TValue> where TInterval : IInterval<TBoundery> where TBoundery : IComparable<TBoundery>
    {
        public readonly IInterval<TBoundery> Interval;
        public readonly TValue[] Values;
        public readonly IntervalNode<TInterval, TBoundery,TValue>[] ChildNodes;

        public IntervalNode(IInterval<TBoundery> interval, TValue[] values, IntervalNode<TInterval, TBoundery,TValue>[] childNodes)
        {
            if (interval == null) throw new ArgumentNullException("interval");
            if (values == null) throw new ArgumentNullException("values");
            if (childNodes == null) throw new ArgumentNullException("childNodes"); 

            Interval = interval;
            Values = values;
            ChildNodes = childNodes;
        }        

        public static IntervalNode<TInterval, TBoundery, TValue> NewNodeWithChildren(IInterval<TBoundery> interval, TValue[] values)
        {
            return new IntervalNode<TInterval, TBoundery, TValue>(interval, values, EmptyChildSet);
        }

        public IntervalNode<TInterval, TBoundery, TValue> WithNode(IntervalNode<TInterval, TBoundery, TValue> node)
        {
            //this is A, 'node' is B
            var @case = IntervalClassification.Evaluate<TInterval, TBoundery>(Interval, node.Interval);
            switch (@case)
            {
                case IntervalCase.Same: 
                    return new IntervalNode<TInterval, TBoundery, TValue>(Interval, Values.Concat(node.Values).ToArray(), ChildNodes);
                case IntervalCase.AinB:
                    return node.WithNode(this);
                case IntervalCase.BinA:
                    return OtherInjectedIntoThis(node);
                case IntervalCase.Unconnected:
                    IInterval<TBoundery> compoundInterval = Interval.Compound(node.Interval);
                    return new IntervalNode<TInterval, TBoundery, TValue>(compoundInterval, EmptyValueSet, new[] { this, node });
                case IntervalCase.Intersecting:
                    return ThisInjectedIntoOther(node);
                default: throw new InvalidOperationException("I messed up");
            }
        }
        
        private IntervalNode<TInterval, TBoundery, TValue> OtherInjectedIntoThis(IntervalNode<TInterval, TBoundery, TValue> node)
        {
            var parent = ChildNodes.FirstOrDefault(c => node.Interval.IsWithin<TInterval, TBoundery>(c.Interval));
            if (parent.IsDefined)
            {
                var changed = parent.WithNode(node);
                return new IntervalNode<TInterval, TBoundery, TValue>(Interval, Values,
                    ChildNodes.Where(c => !c.Equals(parent)).Concat(Enumerable.Repeat(changed, 1)).ToArray());
            }

            var same = ChildNodes.FirstOrDefault(c => node.Interval.IsSameAs<TInterval, TBoundery>(c.Interval));
            if(same.IsDefined)
            {
                var changed = new IntervalNode<TInterval, TBoundery, TValue>(same.Interval, 
                    same.Values.Concat(node.Values).ToArray(), same.ChildNodes);
                return new IntervalNode<TInterval, TBoundery, TValue>(Interval, Values,
                    ChildNodes.Where(c => !c.Equals(same)).Concat(Enumerable.Repeat(changed, 1)).ToArray());
            }

            return new IntervalNode<TInterval, TBoundery, TValue>(Interval, Values,
                                ChildNodes.Concat(Enumerable.Repeat(node, 1)).ToArray());
                
        }

        private IntervalNode<TInterval, TBoundery, TValue> ThisInjectedIntoOther(IntervalNode<TInterval, TBoundery, TValue> node)
        {
            IntervalCase endCase;
            foreach(var child in ChildNodes)
            {
                endCase = IntervalClassification.Evaluate<TInterval, TBoundery>(child.Interval, node.Interval);
                if(endCase == IntervalCase.AinB)
                {
                    var changed = child.WithNode(node);
                    return new IntervalNode<TInterval, TBoundery, TValue>(Interval, Values,
                        ChildNodes.Where(c => !c.Equals(child)).Concat(Enumerable.Repeat(changed, 1)).ToArray());
                }                    
                if (endCase == IntervalCase.BinA)
                    return node.WithNode(this);
            }
            return new IntervalNode<TInterval, TBoundery, TValue>(Interval.Compound(node.Interval), EmptyValueSet, new[] { this, node });
        }

        public IntervalNode<TInterval, TBoundery, TValue> RemoveNode(IntervalNode<TInterval, TBoundery, TValue> node)
        {
            return new IntervalNode<TInterval, TBoundery, TValue>(Interval, Values, ChildNodes.Where(c=>!c.Equals(node)).ToArray());
        }

        internal IntervalNode<TInterval, TBoundery, TValue> WithValueRemovedAt(TInterval targetInterval, params TValue[] valuesToRemove)
        {
            var @case = IntervalClassification.Evaluate<TInterval, TBoundery>(Interval, targetInterval);
            switch (@case)
            {
                case IntervalCase.Same:
                    return new IntervalNode<TInterval, TBoundery, TValue>(Interval, Values.Except(valuesToRemove).ToArray(), ChildNodes);
                case IntervalCase.BinA:
                    throw new NotImplementedException("Delegate, but rebuild index");
                default:
                    return this;
            }
        }

        private static TValue[] EmptyValueSet = new TValue[0];
        private static IntervalNode<TInterval, TBoundery, TValue>[] EmptyChildSet = new IntervalNode<TInterval, TBoundery, TValue>[0];

        public int Count { get { return ChildNodes.Sum(c=>c.Count) + 1; } }
        public bool IsDefined {  get{ return !this.Equals(default(IntervalNode<TInterval, TBoundery, TValue>)); } }
    }

    internal enum IntervalCase
    {
        Same, Unconnected, AinB, BinA, Intersecting
    }

    internal static class IntervalClassification
    {
        public static IntervalCase Evaluate<TInterval, TBoundery>(IInterval<TBoundery> a, IInterval<TBoundery> b) 
            where TInterval : IInterval<TBoundery> where TBoundery : IComparable<TBoundery>
        {
            if (a.IsSameAs<TInterval, TBoundery>(b))
                return IntervalCase.Same;
            if (a.IsWithin<TInterval, TBoundery>(b))
                return IntervalCase.AinB;
            if (b.IsWithin< TInterval, TBoundery>(a))
                return IntervalCase.BinA;
            if (b.IsIntersecting<TInterval,TBoundery>(a))
                return IntervalCase.Intersecting;
            return IntervalCase.Unconnected;
        }

        public static bool IsWithin<TInterval, TBoundery>(this IInterval<TBoundery> a, IInterval<TBoundery> b)
            where TInterval : IInterval<TBoundery> where TBoundery : IComparable<TBoundery>
        {
            return
                (a.From.CompareTo(b.From) > 0) &&
                (a.To.CompareTo(b.To) < 0);
        }

        public static bool IsSameAs<TInterval, TBoundery>(this IInterval<TBoundery> a, IInterval<TBoundery> b)
    where TInterval : IInterval<TBoundery> where TBoundery : IComparable<TBoundery>
        {
            return
                (a.From.CompareTo(b.From) == 0) &&
                (a.To.CompareTo(b.To) == 0);
        }

        public static bool IsIntersecting<TInterval, TBoundery>(this IInterval<TBoundery> a, IInterval<TBoundery> b)
    where TInterval : IInterval<TBoundery> where TBoundery : IComparable<TBoundery>
        {
            return
                (b.From.CompareTo(a.From) >= 0 && b.From.CompareTo(a.To) <= 0) ||
                (b.To.CompareTo(a.From) >= 0 && b.To.CompareTo(a.To) <= 0);
        }
    }
}

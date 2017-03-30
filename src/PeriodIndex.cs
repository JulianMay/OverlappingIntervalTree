//using Spectra.Rates.Model;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace QuadTree
//{


//    class Node
//    {
//        public readonly int Id;
//        public readonly Period Period;
//        public List<Node> ChildNodes = new List<Node>();

//        public Node(int id, Period period)
//        {
//            Id = id;
//            Period = period;
//        }

//        public override string ToString()
//        {
//            return $"{Period} (id {Id})";
//        }

//        public IEnumerable<Node> GetIntersectingNodes(Period queryPeriod, Action<int> reached)
//        {
//            reached(Id);

//            if (!queryPeriod.IntersectsWith(Period))
//                yield break;

//            yield return this;



//            foreach (var n in ChildNodes.SelectMany(c => c.GetIntersectingNodes(queryPeriod, reached)))
//                yield return n;
//        }



//        public void AddNode(Node other)
//        {
//            var parent = ChildNodes.FirstOrDefault(c => other.Period.IsWithin(c.Period));
//            if(parent != null)
//            {
//                parent.AddNode(other);
//                return;
//            }

//            ChildNodes.Add(other);
//        }     
//    }

//    class PeriodIndex
//    {
//        private Node _root = null;
//        private Action<int> _onReached;

//        private int counter = 0;

//        public PeriodIndex(Action<int> onReached)
//        {
//            _onReached = onReached;
//        }

//        public IEnumerable<Node> GetIntersectingNodes(Period queryPeriod)
//        {
//            return _root.GetIntersectingNodes(queryPeriod, _onReached);
//        }

//        public int AddNode(Period p)
//        {
//            var newNode = new Node(counter++, p);

//            if (_root == null)
//            {
//                _root = newNode;
//                return newNode.Id;
//            }

//            if (newNode.Period.IsWithin(_root.Period))
//                _root.AddNode(newNode);
//            else
//                _root = NewContainingNode(_root, newNode, () => counter++);

//            return newNode.Id;
//        }

//        private Node NewContainingNode(Node _root, Node newNode, Func<int> idGenerator)
//        {
//            var period = new Period(_root.Period.EarliestBeginning(newNode.Period), _root.Period.LatestEnd(newNode.Period));
//            return new Node(idGenerator(), period);
//        }
//    }

//}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*
namespace DeepLearning
{
    public class MCNode<TGame, TMove>
        where TGame : IEnvironment<TMove>
    {
        public ReadOnlyCollection<MCNode<TGame, TMove>> Children => _children.AsReadOnly();
        public MCNode<TGame, TMove> Parent { get; private init; }
        public bool IsRoot => Parent == null;
        public bool IsLeaf => _children.Any();
        public ReadOnlyCollection<TMove> MoveSequence => _moveSequence.AsReadOnly();

        private int _visits;
        private double _totalValue;
        private readonly List<MCNode<TGame, TMove>> _children = new();
        private readonly List<TMove> _moveSequence;

        public MCNode(MCNode<TGame, TMove> parent, TMove move, double rolloutValue)
        {
            Parent = parent;
            parent.AddChild(this);
            _moveSequence = parent._moveSequence.ToList();
            _moveSequence.Add(move);
            _totalValue = rolloutValue;
            _visits = 1;
        }

        public void AddChild(MCNode<TGame, TMove> child)
            => _children.Add(child);

        public double Appeal()
        {
            if (IsRoot)
                throw new Exception($"Could not calculate the appeal of a root node");
            return _totalValue / _visits + 2 * Math.Sqrt(Math.Log(Parent._visits) / _visits);
        }
    }
}
*/
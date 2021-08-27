using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace DeepLearning
{
    public struct Moment<TMove>
    {
        public readonly Vector<double> Before;
        public readonly TMove Move;
        public readonly Vector<double> After;
        public readonly double Reward;
        public readonly bool IsTerminal;
        public readonly ReadOnlyCollection<TMove> ValidMoves;

        public Moment(Vector<double> before, TMove move, Vector<double> after, double reward, bool isTerminal, IEnumerable<TMove> validMoves)
        {
            Before = before;
            Move = move;
            After = after;
            Reward = reward;
            IsTerminal = isTerminal;
            ValidMoves = validMoves.ToList().AsReadOnly();
        }
    }
}

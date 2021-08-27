using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace DeepLearning
{
    public interface IEnvironment<TMove>
    {
        public int LayerSize { get; }
        public ReadOnlyCollection<TMove> AllPossibleMoves { get; }
        public bool IsValidMove(TMove move);
        public TMove Hint(out bool gaveHint);
        public void MakeMove(TMove move);
        public double Reward();
        public bool IsTerminal();
        public bool HasWon();
        public bool HasLost();
        public bool HasTimedOut();
        public void Reset();
        public Vector<double> ToLayer();
        public string MoveToString(TMove move);
    }
}

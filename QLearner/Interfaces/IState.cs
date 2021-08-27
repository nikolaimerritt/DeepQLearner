using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using System.Threading.Tasks;

namespace DeepLearning
{
    public interface IState<TMove>
    {
        public List<TMove> ValidMoves();
        public IState<TMove> MakeMove(TMove move);
        public bool IsWinState();
        public bool IsLossState();
    }
}

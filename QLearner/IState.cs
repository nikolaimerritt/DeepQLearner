using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace DeepLearning
{
    using Vector = Vector<double>;

    public interface IState<TAction>
    {
        public List<TAction> AllPossibleActions { get; }
        public int InputLayerSize { get; }
        public Vector ToInputLayer();
        public double Reward();
        public bool IsTerminalState();
        // public bool ActionsAreInverses(TAction action, TAction prevAction);
        public bool IsValidAction(TAction action);
        public IState<TAction> AfterAction(TAction action);
    }
}

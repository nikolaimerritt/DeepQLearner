using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using NeuralNetLearning;
using System.Threading;
using MathNet.Numerics.LinearAlgebra;

namespace DeepLearning
{
    using Vector = Vector<double>;
    public class QLearner<T>
    {
        private readonly NeuralNet _net;
        private readonly Random _rng = new();
        private readonly double _learningRate = 0.5;
        private readonly double _futureDiscount = 0.9;
        private readonly IState<T> _initialState;
        private static readonly int _maxMemorySize = 500;
        private readonly List<(IState<T>, T, IState<T>)> _memory = new(_maxMemorySize);

        public QLearner(IState<T> initialState)
        {
            _initialState = initialState;

            int[] layerSizes = LayerSizes(_initialState.InputLayerSize, _initialState.AllPossibleActions.Count, layerCount: 4);

            var layers = new NeuralLayerConfig[]
            {
                new InputLayer(layerSizes[0]),
                new HiddenLayer(layerSizes[1], new ReluActivation()),
                new HiddenLayer(layerSizes[2], new ReluActivation()),
                new OutputLayer(layerSizes[3], new IdentityActivation())
            };

            var descender = new StochasticGradientDescender(learningRate: 0.001);
            var cost = new MSECost();
            _net = NeuralNetFactory.RandomCustomisedForRelu(layers, descender, cost);
        }

        private static int[] LayerSizes(int inputLayerSize, int outputLayerSize, int layerCount)
        {
            double start = inputLayerSize;
            double step = 1.0 / (layerCount - 1.0);
            double end = outputLayerSize;

            return Enumerable
                .Range(0, layerCount)
                .Select(i => (int)(start + i * step * (end - start)))
                .ToArray();
        }

        private T ChooseBestAction(IState<T> state)
        {
            Vector qvalues = _net.Output(state.ToInputLayer());
            for (int i = 0; i < qvalues.Count; i++)
            {
                T action = state.AllPossibleActions[i];
                if (!state.IsValidAction(action))
                    qvalues[i] = double.NegativeInfinity;
            }
            var bestIndices = Enumerable.Range(0, qvalues.Count)
                .Where(i => qvalues[i] == qvalues.Max())
                .ToList();

            return state.AllPossibleActions[bestIndices[_rng.Next(bestIndices.Count)]];
        }

        private T ChooseRandomAction(IState<T> state)
        {
            var validActions = state.AllPossibleActions
                .Where(state.IsValidAction)
                .ToList();
            return validActions[_rng.Next(validActions.Count)];
        }

        private double QValue(IState<T> state, T action)
        {
            if (!state.IsValidAction(action))
                return double.NegativeInfinity;

            Vector qvalues = _net.Output(state.ToInputLayer());
            return qvalues[ActionIndex(state, action)];
        }

        private double NewQValue(IState<T> before, T action, IState<T> after)
        {
            double oldQValue = QValue(before, action);
            double rewardAfter = after.Reward();
            double futureQValue = after.IsTerminalState()
                ? rewardAfter
                : after.AllPossibleActions
                .Where(after.IsValidAction)
                .Select(a => QValue(after, a))
                .Max();

            return (1 - _learningRate) * oldQValue + _learningRate * (rewardAfter + _futureDiscount * futureQValue);
        }

        private void LearnFromMemory()
        {
            List<(Vector, Vector)> trainingPairs = new(_memory.Count);
            foreach ((var before, var action, var after) in _memory)
            {
                Vector input = before.ToInputLayer();
                Vector desiredOutput = _net.Output(input);
                desiredOutput[ActionIndex(before, action)] = NewQValue(before, action, after);
                trainingPairs.Add((input, desiredOutput));
            }
            Shuffle(trainingPairs);
            _net.GradientDescent(trainingPairs, batchSize: 256, numEpochs: 20);
        }

        private static int ActionIndex(IState<T> state, T action)
            => state.AllPossibleActions.FindIndex(act => act.Equals(action));

        private double ExploreProbability(int gameNum, int numGames, double maxProb, double minProb)
            => minProb + gameNum * (maxProb - minProb) / numGames;

        private void LearnFromGame(double exploreProbability, ref int deaths, ref int cheeses)
        {
            IState<T> state = _initialState;
            while (!state.IsTerminalState())
            {
                IState<T> before = state;
                T action = _rng.Next() <= exploreProbability ? ChooseRandomAction(before) : ChooseBestAction(before);
                IState<T> after = before.AfterAction(action);
                _memory.Add((before, action, after));

                if (_memory.Count == _maxMemorySize)
                {
                    LearnFromMemory();
                    _memory.Clear();
                }
                state = after;
                if (state.IsTerminalState())
                {
                    if (state.Reward() >= 10)
                        cheeses++;
                    else if (state.Reward() <= -10)
                        deaths++;
                }
            }
        }

        public void Learn(int numGames)
        {
            int deaths = 0;
            int cheeses = 0;
            int updateFrequency = 100;

            for (int gameNum = 1; gameNum <= numGames; gameNum++)
            {
                LearnFromGame(ExploreProbability(gameNum, numGames, maxProb: 0.9, minProb: 0.01), ref deaths, ref cheeses);

                if (gameNum % updateFrequency == 0)
                {
                    Console.WriteLine($"game num: {gameNum} / {numGames} \t deaths: {deaths}, cheeses: {cheeses}");
                    deaths = 0;
                    cheeses = 0;
                }
            }
        }

        public void PlayDemoGame()
        {
            IState<T> state = _initialState;
            Console.WriteLine(state);
            while (!state.IsTerminalState())
            {
                T action = ChooseBestAction(state);
                Thread.Sleep(1000);
                Console.Clear();
                state = state.AfterAction(action);
                Console.WriteLine(state);
            }
        }

        private void Shuffle<U>(IList<U> list)
        {
            int swapIdx = list.Count;
            while (swapIdx > 1)
            {
                swapIdx--;
                int replaceIdx = _rng.Next(swapIdx + 1);
                U value = list[replaceIdx];
                list[replaceIdx] = list[swapIdx];
                list[swapIdx] = value;
            }
        }
    }
}
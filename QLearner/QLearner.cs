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
    using TrainingPair = Tuple<Vector<double>, Vector<double>>;
    public class QLearner<TEnvironment, TMove>
        where TEnvironment : IEnvironment<TMove>
    {
        private static readonly Random _rng = new();

        private readonly NeuralNet _net;
        private readonly TEnvironment _environment; 
        private static readonly int _maxMemorySize = 6000;
        private static readonly double _learningRate = 0.1;

        public QLearner(TEnvironment environment, NeuralNet net)
        {
            _environment = environment;
            _net = net;
        }

        private static NeuralNet DefaultNet(TEnvironment environment)
        {
            int moveCount = environment.AllPossibleMoves.Count;
            var layers = new NeuralLayerConfig[]
            {
                new InputLayer(environment.LayerSize),
                new HiddenLayer(environment.LayerSize / 2, new ReluActivation(leak: 0.01)),
                new HiddenLayer(environment.LayerSize / 2, new ReluActivation(leak: 0.01)),
                new OutputLayer(layerSize: moveCount, new IdentityActivation())
            };

            var descender = new AdamGradientDescender(learningRate: 1e-4);
            var cost = new MSECost();
            return NeuralNetFactory.RandomCustomisedForRelu(layers, descender, cost);
        }

        public QLearner(TEnvironment environment)
            : this(environment, DefaultNet(environment))
        { }

        private TMove ChooseExploreExploitMove(TEnvironment environment, double exploreProbability)
        {
            // augmentation
            TMove hint = environment.Hint(out bool gaveHint);
            if (gaveHint)
                return hint;
            else if (_rng.NextDouble() < exploreProbability)
                return ChooseRandomMove(environment);
            else 
                return ChooseBestMove(environment);
        }

        public TMove MakeMove(TEnvironment environment)
        {
            TMove hint = environment.Hint(out bool gaveHint);
            return gaveHint ? hint : ChooseBestMove(environment);
        }

        private TMove ChooseBestMove(TEnvironment environment)
        {
            List<TMove> validMoves = environment.AllPossibleMoves.Where(environment.IsValidMove).ToList();
            TMove bestMove = default;
            double highestQValue = double.NegativeInfinity;
            foreach (TMove move in validMoves)
            {
                double qvalue = QValue(environment.ToLayer(), move);
                if (qvalue > highestQValue)
                {
                    highestQValue = qvalue;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        private static TMove ChooseRandomMove(TEnvironment environment)
        {
            var validMoves = environment.AllPossibleMoves.Where(environment.IsValidMove).ToList();
            return validMoves[_rng.Next(validMoves.Count)];
        }

        private Vector<double> QValues(Vector<double> environmentLayer)
            => _net.Output(environmentLayer);

        private double QValue(Vector<double> environmentLayer, TMove move)
        {
            var qvalues = QValues(environmentLayer);
            double qvalue = qvalues[IndexOf(move)];
            return qvalue;
        }

        private double NewQValue(Moment<TMove> moment, double discountFactor)
        {
            double oldQValue = QValue(moment.Before, moment.Move);
            double temporalDifference = moment.IsTerminal ?
                moment.Reward - oldQValue :
                moment.Reward + discountFactor * QValues(moment.After).Max() - oldQValue;

            return oldQValue + _learningRate * temporalDifference;
        }

        private void LearnFromMemory(List<Moment<TMove>> memory, double futureDiscount)
        {
            if (!memory.Any())
                throw new ArgumentException($"Could not learn from an empty memory list");

            Shuffle(memory);
            List<TrainingPair> trainingPairs = new(capacity: memory.Count);
            foreach (var moment in memory)
            {
                Vector input = moment.Before;
                Vector desiredOutput = QValues(input);
                desiredOutput[IndexOf(moment.Move)] = NewQValue(moment, futureDiscount);
                trainingPairs.Add(new(input, desiredOutput));
            }
            _net.GradientDescent(trainingPairs, batchSize: 256, numEpochs: 10, computeBatchGradientInParallel: true);
        }

        public void Learn(int totalGames, string directoryPath)
        {
            int printFrequency = totalGames / 100;
            double exploreProbability;
            double futureDiscount;
            List<Moment<TMove>> memory = new(capacity: _maxMemorySize);

            for (int gameNum = 1; gameNum <= totalGames; gameNum++)
            {
                exploreProbability = ExploreProbability(gameNum, totalGames);
                futureDiscount = gameNum < (int)5e4 ? // future q values will be complete garbage at first, and likely outside [-1, 1]
                    0 :
                    1 - exploreProbability;

                if (gameNum % printFrequency == 0)
                {
                    Console.WriteLine($"Game {gameNum} / {totalGames} \t exploring {100 * exploreProbability:0.00}% \t future q-values decay rate: {futureDiscount:0.000}");
                    if (gameNum % (10 * printFrequency) == 0 || gameNum == totalGames)
                    {
                        Console.Write($"\nSaving QLearner to directory {directoryPath}...\t");
                        WriteToDirectory(directoryPath);
                        Console.WriteLine("saved");
                    }
                }

                while (!_environment.IsTerminal())
                {
                    Vector before = _environment.ToLayer();
                    TMove move = ChooseExploreExploitMove(_environment, exploreProbability);
                    _environment.MakeMove(move);

                    Moment<TMove> moment = new(
                        before, 
                        move, 
                        _environment.ToLayer(),
                        _environment.Reward(), 
                        _environment.IsTerminal(), 
                        _environment.AllPossibleMoves.Where(_environment.IsValidMove));
                    memory.Add(moment);

                    if (memory.Count >= _maxMemorySize)
                    {
                        LearnFromMemory(memory, futureDiscount);
                        memory.Clear();
                    }
                }
                _environment.Reset();
            }
        }

        private int IndexOf(TMove move)
            => _environment.AllPossibleMoves.IndexOf(move);

        public void WriteToDirectory(string directoryPath)
            => _net.WriteToDirectory(directoryPath);

        public static QLearner<TEnvironment, TMove> ReadFromDirectory(TEnvironment environment, string directoryPath)
            => new QLearner<TEnvironment, TMove>(environment, NeuralNet.ReadFromDirectory(directoryPath));

        private void ShowState(int gameNum, int totalGames, double exploreProb)
        {
            Console.Clear();
            Console.WriteLine($"Game {gameNum} / {totalGames} \t\t explore prob: {100 * exploreProb:0.00}%\n");
            Console.WriteLine(_environment);
            Console.WriteLine("\n\n");

            foreach (TMove move in _environment.AllPossibleMoves.Where(_environment.IsValidMove))
              Console.Write($"{move}: {QValue(_environment.ToLayer(), move):0.000000} \t");
            Console.ReadKey();
            Console.WriteLine();
        }

        private TMove ReadActionInput()
        {
            var validMoves = _environment.AllPossibleMoves.Where(_environment.IsValidMove).ToList();

            for (int i = 0; i < validMoves.Count; i++)
                Console.Write($"{i}: {validMoves[i]} \t");

            int indexInputted = int.Parse(Console.ReadLine());
            return validMoves[indexInputted];
        }

        public void ShowQValues()
        {
            foreach (TMove mv in _environment.AllPossibleMoves.Where(_environment.IsValidMove))
                Console.Write($"{_environment.MoveToString(mv)}: {QValue(_environment.ToLayer(), mv):0.0000} \t");
            Console.WriteLine("\n\n");
        }

        public void ShowDemo()
        {
            Console.WriteLine(_environment);
            ShowQValues();
            
            while (!_environment.IsTerminal())
            {
                TMove move = ReadActionInput();
                Console.Clear();
                _environment.MakeMove(move);
                Console.WriteLine(_environment);

                ShowQValues();
            }
        }


        private static double ExploreProbability(int gameNum, int totalGames)
        {
            double gameProportion = (double)gameNum / totalGames;
            double maxProb = 0.2;
            double minProb = 0.01;
            double halfLife = 0.3;

            double decay = Math.Log(2 / (1 + minProb / maxProb)) / halfLife;
            return Math.Max(minProb, maxProb * Math.Exp(-decay * gameProportion));
        }

        private static List<Vector<double>> GetInputVectors(TEnvironment environment, int numVectors)
        {
            List<Vector<double>> inputVectors = new(numVectors);
            for (int i = 1; i <= numVectors; i++)
            {
                if (!environment.IsTerminal())
                {
                    TMove move = ChooseRandomMove(environment);
                    inputVectors.Add(environment.ToLayer());
                    environment.MakeMove(move);
                }
                else environment.Reset();
            }
            environment.Reset();
            return inputVectors;
        }


        public static void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = _rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

    
}
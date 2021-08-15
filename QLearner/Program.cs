/*
using System;
using NeuralNetLearning;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NeuralNetLearning.Maths;
using System.IO;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Data.Text;
using System.Runtime.InteropServices;

namespace DeepLearning
{    
    using Vector = Vector<double>;

    [StructLayout(LayoutKind.Explicit)]
    struct Converter
    {
        [FieldOffset(0)] public double d;
        [FieldOffset(0)] public int i;

        public static double Beast(double x)
        {
            Converter c;
            c.i = 0;
            c.d = x;
            System.Random rng = new Random(c.i);

            return rng.Next();
        }
    }

    class Program
    {
        private static List<(Vector, Vector)> GetTrainingPairs(int count)
        {
            List<(Vector, Vector)> trainingPairs = new(count);
            for (int i = 0; i < count; i++)
            {
                Vector input = 50 * VectorFunctions.StdUniform(dim: 1);
                // Vector output = input.PointwisePower(2);
                Vector output = input.ApplyPointwise(Converter.Beast);
                trainingPairs.Add((input, output));
            }
            return trainingPairs;
        }

        public static NeuralNet TestNet()
        {
            var layerConfigs = new NeuralLayerConfig[]
            {
                new InputLayer(1),
                new HiddenLayer(80, new ReluActivation()),
                new HiddenLayer(80, new ReluActivation()),
                new OutputLayer(1, new IdentityActivation())
            };

            var gradDescender = new AdamGradientDescender();
            var cost = new HuberCost(outlierBoundary: 100);
            var firstMiniBatch = GetTrainingPairs(1000);
            NeuralNet net = NeuralNetFactory.RandomCustomisedForMiniBatch(layerConfigs, firstMiniBatch, gradDescender, cost);
            return net;
        }

        private static void WriteResults(NeuralNet net, string filepath)
        {
            string results = "";
            foreach ((Vector input, Vector desiredOutput) in GetTrainingPairs(10))
            {
                string padding = input[0] >= 0 ? " " : "";
                results += $"{padding}{input[0]:0.00} \t ---> \t {net.Output(input)[0]:0.00} \t (wanted: {desiredOutput[0]:0.00}) \n";
            }
            File.WriteAllText(filepath, results);
        }
            
        public static void Main()
        {
            Vector first = Vector.Build.DenseOfArray(new double[] { 1, 2, 3 });
            Vector second = Vector.Build.DenseOfEnumerable(Enumerable.Range(1, 3).Select(x => (double)x));
            Console.WriteLine(first.Equals(second));
        }
    }
} */

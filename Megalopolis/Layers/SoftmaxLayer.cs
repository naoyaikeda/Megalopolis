﻿using System;
using System.Collections.Generic;
using Megalopolis.ActivationFunctions;

namespace Megalopolis
{
    namespace Layers
    {
        public class SoftmaxLayer : Layer
        {
            public SoftmaxLayer(int inputs, int outputs, Func<int, int, int, double> func) : base(inputs, outputs)
            {
                var length = inputs * outputs;

                this.weights = new double[length];
                this.biases = new double[outputs];

                for (int i = 0; i < length; i++)
                {
                    this.weights[i] = func(i, inputs, outputs);
                }

                for (int i = 0; i < outputs; i++)
                {
                    this.biases[i] = 0;
                }
            }

            public SoftmaxLayer(SoftmaxLayer layer) : base(layer)
            {
                this.weights = new double[layer.weights.Length];
                this.biases = new double[layer.biases.Length];

                for (int i = 0; i < layer.weights.Length; i++)
                {
                    this.weights[i] = layer.weights[i];
                }

                for (int i = 0; i < layer.biases.Length; i++)
                {
                    this.biases[i] = layer.biases[i];
                }
            }

            public SoftmaxLayer(SoftmaxLayer sourceLayer, Layer targetLayer) : base(sourceLayer, targetLayer)
            {
                this.weights = new double[sourceLayer.weights.Length];
                this.biases = new double[sourceLayer.biases.Length];

                for (int i = 0; i < sourceLayer.weights.Length; i++)
                {
                    this.weights[i] = sourceLayer.weights[i];
                }

                for (int i = 0; i < sourceLayer.biases.Length; i++)
                {
                    this.biases[i] = sourceLayer.biases[i];
                }
            }

            public override void PropagateForward(bool isTraining)
            {
                double[] summations = new double[this.outputActivations.Length];

                for (int i = 0; i < this.outputActivations.Length; i++)
                {
                    double sum = 0;

                    for (int j = 0; j < this.inputActivations.Length; j++)
                    {
                        sum += this.inputActivations[j] * this.weights[this.outputActivations.Length * j + i];
                    }

                    sum += this.biases[i];

                    summations[i] = sum;
                }

                for (int i = 0; i < this.outputActivations.Length; i++)
                {
                    this.outputActivations[i] = Softmax(summations, i);
                }
            }

            public override IEnumerable<double[]> PropagateBackward(ref double[] deltas, out double[] gradients)
            {
                var d = new double[this.inputActivations.Length];

                gradients = new double[this.inputActivations.Length * this.outputActivations.Length];

                for (int i = 0, j = 0; i < this.inputActivations.Length; i++)
                {
                    double error = 0;

                    for (int k = 0; k < this.outputActivations.Length; k++)
                    {
                        error += deltas[k] * this.weights[j];
                        gradients[j] = deltas[k] * this.inputActivations[i];
                        j++;
                    }

                    d[i] = error;
                }

                return new double[][] { deltas, d };
            }

            public override void Update(double[] gradients, double[] deltas, Func<double, double, double> func)
            {
                var length = this.inputActivations.Length * this.outputActivations.Length;

                for (int i = 0; i < length; i++)
                {
                    this.weights[i] = func(this.weights[i], gradients[i]);
                }

                for (int i = 0; i < this.outputActivations.Length; i++)
                {
                    this.biases[i] = func(this.biases[i], deltas[i]);
                }
            }

            public override Layer Copy()
            {
                return new SoftmaxLayer(this);
            }

            public override Layer Copy(Layer layer)
            {
                return new SoftmaxLayer(this, layer);
            }

            private double Softmax(double[] x, int i)
            {
                double max = 0;
                double sum = 0;

                for (int j = 0; j < x.Length; j++)
                {
                    if (x[j] > max)
                    {
                        max = x[j];
                    }
                }

                for (int j = 0; j < x.Length; j++)
                {
                    sum += Math.Exp(x[j] - max);
                }

                return Math.Exp(x[i] - max) / sum;
            }

            private double[] DerivativeOfSoftmax(double[] x, int i)
            {
                // yi(1 - yi) if i = j
                // -yiyj otherwise
                double[] vector = new double[x.Length];

                for (int j = 0; j < x.Length; j++)
                {
                    if (i == j)
                    {
                        vector[j] = x[i] * (1.0 - x[i]);
                    }
                    else
                    {
                        vector[j] = -x[j] * x[i];
                    }
                }

                return vector;
            }
        }
    }
}

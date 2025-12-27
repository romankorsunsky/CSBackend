

using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;

namespace b1.Main
{
    public class GBMValueGenerator: IValueGenerator
    {
        //those(these ?) two arrays are just numbers that make sense for paremeters in the context of Geometric Brownian Motion formual
        private readonly double[] _muSource = { 0.0140, 0.0145, 0.0150, 0.01550, 0.0160, 0.01650, 0.0170, 0.0175, 0.0180, 0.0185 };
        private readonly double[] _sigmaSource = { 0.140, 0.145, 0.150, 0.1550, 0.160, 0.1650, 0.170, 0.175, 0.180, 0.185 };

        private double Mu { get; set; }
        private double Sigma { get; set; }
        private double Delta { get; set; }

        public GBMValueGenerator()
        {
            var rnd = new Random();
            var muIndex = rnd.Next(0, 10);
            var sigmaIndex = rnd.Next(0, 10);
            Delta = 1.0 / 365;
            Mu = _muSource[muIndex];
            Sigma = _sigmaSource[sigmaIndex];
        }
        public GBMValueGenerator WithMu(double mu)
        {
            Mu = mu;
            return this;
        }
        public GBMValueGenerator WithSigma(double sigma)
        {
            Sigma = sigma;
            return this;
        }

        public GBMValueGenerator WithDelta(double delta)
        {
            Delta = delta;
            return this;
        }
        public double GetValue(double prev)
        {
            const double e = MathNet.Numerics.Constants.E;
            var normal = new Normal();
            double sqrt_dt = Math.Sqrt(Delta);
            double m = Mu - 0.5 * Math.Pow(Sigma, 2);
            double Z = normal.Sample();
            double next = prev * Math.Pow(e, (m * Delta) + Sigma * sqrt_dt * Z);
            return next;
        }
        public int GetValue(int prev)
        {
            return (int)GetValue((double)prev);
        }
    }
}
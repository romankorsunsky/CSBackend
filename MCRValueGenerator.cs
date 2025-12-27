using MathNet.Numerics.Distributions;
using ScottPlot.Colormaps;

namespace b1.Main
{
    //Mean Correcting Random Value Generator, basically oscilates but stays around some mean, mu
    public class MCRValueGenerator : IValueGenerator
    {
        public double Theta { get; set; }
        public double Mean { get; set; }
        public double Sigma { get; set; }
        public double Delta { get; set; }
        public MCRValueGenerator()
        {
            Theta = 0.003;
            Mean = 1.0;
            Sigma = 0.0005;
            Delta = 1.0/252;
        }
        public MCRValueGenerator WithTheta(double theta)
        {
            Theta = theta;
            return this;
        }
        public MCRValueGenerator WithMean(double mean)
        {
            Mean = mean;
            return this;
        }
        public MCRValueGenerator WithSigma(double sigma)
        {
            Sigma = sigma;
            return this;
        }
        public MCRValueGenerator WithDelta(double delta)
        {
            Delta = delta;
            return this;
        }
        public double GetValue(double prev)
        {
            var normal = new Normal();
            var volume = prev + Theta * (Mean - prev) + Sigma * Math.Sqrt(Delta) * normal.Sample();

            return volume < 0 ? (volume * (-1)) : volume;
        }

        public int GetValue(int prev)
        {
            return (int)GetValue((double)prev);
        }
    }
}
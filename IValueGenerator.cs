namespace b1.Main
{
    public interface IValueGenerator
    {
        public double GetValue(double prev);

        public int GetValue(int prev);
    }
}
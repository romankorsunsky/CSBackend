using System.Collections.Concurrent;

namespace b1.Main
{
    public sealed class ValueGeneratorFactory
    {
        private static readonly ValueGeneratorFactory _instance = new ValueGeneratorFactory();

        static ValueGeneratorFactory(){}
        private ConcurrentDictionary<string, Func<IValueGenerator>> ValGenMap { get; init; } 
        private ValueGeneratorFactory()
        {
            ValGenMap = new ConcurrentDictionary<string, Func<IValueGenerator>>();
        }
        public static ValueGeneratorFactory GetInstance()
        {
            return _instance;
        }
        //returns null if no matching ValueGenerator 
        public IValueGenerator? GetValueGenerator(string generatorType)
        {
            if (ValGenMap.TryGetValue(generatorType, out var maker))
            {
                return maker();
            }
            else
            {
                return null;
            }
        }
        public ICollection<string> GetGeneratorTypes()
        {
            return ValGenMap.Keys;
        }
        
        public void RegisterGenerator(string generatorName, Func<IValueGenerator> instanceMaker)
        {
            if (!(generatorName == null) && generatorName != "" && instanceMaker != null)
                ValGenMap.TryAdd(generatorName, instanceMaker);
        }
    }
}
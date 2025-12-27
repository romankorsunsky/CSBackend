using System.Collections.Concurrent;

namespace b1.Main
{
    public class ValueGeneratorFactory
    {
        private ConcurrentDictionary<string, Func<IValueGenerator>> _map; 
        public ValueGeneratorFactory()
        {
            _map = new ConcurrentDictionary<string, Func<IValueGenerator>>();
        }

        //returns null if no matching ValueGenerator 
        public IValueGenerator GetValueGenerator(string generatorType)
        {
            try
            {
                if (_map.TryGetValue(generatorType, out var maker))
                {
                    return maker();
                }
                else
                {
                    throw new ArgumentNullException("Didn't find the Generator, check arg or register one");
                }
            }
            catch (ArgumentNullException)
            {
                throw;
            }
        }

        public void RegisterGenerator(string generatorName, Func<IValueGenerator> instanceMaker)
        {
            if(!(generatorName == null) && generatorName != "" && instanceMaker != null)
                _map.TryAdd(generatorName, instanceMaker);
        }
    }
}
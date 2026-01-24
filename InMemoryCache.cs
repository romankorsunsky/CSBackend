using System.Collections.Concurrent;

namespace b1.Main
{
    //bad Idea, limited by memory allocated to the JVM
    //no tolerance to failures and outages
    //this is also actually slow because ConcurrentDict sucks
    //Lock free implementations is better will change if I have enough time 
    public class InMemoryCache
    {
        private ConcurrentDictionary<string, string> _dict = new ConcurrentDictionary<string, string>();

        public bool PutValue(string key, string val)
        {
            return _dict.TryAdd(key, val);
        }

        public string? GetValue(string key)
        {
            if (_dict.TryGetValue(key, out var oldVal))
            {
                return oldVal;
            }
            return null;
        }
    }
}
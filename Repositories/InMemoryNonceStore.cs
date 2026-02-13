using System.Collections.Concurrent;
using b1.Models;
using Microsoft.IdentityModel.Tokens;

namespace b1{
    /// <summary>
    /// A super duper bad implementation of a store/cache
    /// No TTL, simplistic locking, need more checks in input,
    /// Not distributed, and many more. Should just use Redis.
    /// </summary>
    public class InMemoryPositionCreationRequestStore
    {
        private ConcurrentDictionary<string, PositionCreationRequest> _store;
        private static InMemoryPositionCreationRequestStore _instance;

        private InMemoryPositionCreationRequestStore()
        {
            _store = new ConcurrentDictionary<string, PositionCreationRequest>();
        }
        static InMemoryPositionCreationRequestStore()
        {
            _instance = new InMemoryPositionCreationRequestStore();
        }
        public static InMemoryPositionCreationRequestStore GetInstance()
        {
            return _instance;
        }

        public bool TryGet(string id, out PositionCreationRequest? request)
        {
            if (_store.TryGetValue(id, out var actualRequest))
            {
                request = actualRequest;
                return true;
            }
            request = null;
            return false;
        }
        public bool TryAdd(string id, PositionCreationRequest request)
        {
            return _store.TryAdd(id, request);
        }
        public void RemoveNonce(string id)
        {
            _store.Remove(id, out _);
        }
    }
}
using System.Collections.Concurrent;
using b1.Controllers;
using b1.Models;
using b1.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace b1{
    /// <summary>
    /// A super duper bad implementation of a store/cache
    /// No TTL, simplistic locking, need more checks in input,
    /// Not distributed, and many more. Should just use Redis.
    /// </summary>
    public class InMemoryPositionVerificationRepo : IPositionVerificationRepository
    {
        private ConcurrentDictionary<string, PositionVerification> _store;
        private static InMemoryPositionVerificationRepo _instance;

        private InMemoryPositionVerificationRepo()
        {
            _store = new ConcurrentDictionary<string, PositionVerification>();
        }
        static InMemoryPositionVerificationRepo()
        {
            _instance = new InMemoryPositionVerificationRepo();
        }
        public static InMemoryPositionVerificationRepo GetInstance()
        {
            return _instance;
        }

        public Task<bool> TryGet(string id, out PositionVerification? verfctn)
        {
            if (_store.TryGetValue(id, out var actualRequest))
            {
                verfctn = actualRequest;
                return Task.FromResult(true);
            }
            verfctn = null;
            return Task.FromResult(false);
        }
        public Task<bool> TryAdd(string id, PositionVerification verfctn)
        {
            return Task.FromResult<bool>(_store.TryAdd(id, verfctn));
        }
        public Task DeleteRequest(string id)
        {
            _store.Remove(id, out _);
            return Task.CompletedTask;
        }
    }
}
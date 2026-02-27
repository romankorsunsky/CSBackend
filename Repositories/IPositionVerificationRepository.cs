using b1.Models;

namespace b1.Repositories{
    public interface IPositionVerificationRepository
    {
        public Task<bool> TryGet(string id, out PositionVerification? verfctn);
        public Task<bool> TryAdd(string id, PositionVerification verfctn);
        public Task DeleteRequest(string id);
    }
}
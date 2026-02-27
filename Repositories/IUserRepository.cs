using b1.Models;

namespace b1.Repositories
{
    public interface IUserRepository
    {
        public Task<User?> GetUserByUsername(string username);
        public Task<User?> GetUserById(string userId);
        public Task AddNewUser(User user);
        public Task<double?> GetUserBalance(string userId);
        public Task UpdateUserBalance(string userId, double newBalance);
    }
}
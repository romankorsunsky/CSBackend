using b1.Models;

namespace b1.Repositories
{
    public interface IUserRepository
    {
        public Task<User?> GetUserByName(string name);
        public Task<User?> GetUserById(string userId);

        public Task AddNewUser(User user);

        public Task<double?> GetUserBalance(string userId);
    }
}
using b1.Models;

namespace b1.Repositories
{
    public interface IUserRepository
    {
        //returns user if exists or null
        public Task<User?> GetUserByName(string name);

        public Task AddNewUser(User user);
    }
}
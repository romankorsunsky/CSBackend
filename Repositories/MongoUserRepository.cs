

using b1.Models;
using MongoDB.Driver;

namespace b1.Repositories
{
    public class MongoUserRepository : IUserRepository
    {
        private IMongoCollection<User> _userCol;
        private string COL_NAME = "users";
        public MongoUserRepository(IMongoDatabase db)
        {
            _userCol = db.GetCollection<User>(COL_NAME);
        }
        public async Task AddNewUser(User user)
        {
            await _userCol.InsertOneAsync(user);
        }

        public async Task<User?> GetUserByName(string name)
        {
            var result = await _userCol.Find(u => u.Username == name).FirstOrDefaultAsync();
            return result;
        }

        public async Task<double?> GetUserBalance(string userId)
        {
            var res = await _userCol.
                Find(user => user.Id == userId).
                Project(user => (double?)user.Balance).
                FirstOrDefaultAsync();
            return res;
        }

        public async Task<User?> GetUserById(string userId)
        {
            var res = await _userCol.Find(user => user.Id == userId).FirstOrDefaultAsync();
            return res;
        }
    }
}
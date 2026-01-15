

using b1.Models;
using MongoDB.Driver;

namespace b1.Repositories
{
    public class MongoUserRepository : IUserRepository
    {
        private IMongoCollection<User> UserCol { get; init; }
        private string COL_NAME = "users";
        public MongoUserRepository(IMongoDatabase db)
        {
            UserCol = db.GetCollection<User>(COL_NAME);
        }
        public async Task AddNewUser(User user)
        {
            await UserCol.InsertOneAsync(user);
        }

        public async Task<User?> GetUserByName(string name)
        {
            var result = await UserCol.FindAsync(u => u.Username == name,
                 new FindOptions<User, User>() { Limit = 1 });
            var user = await result.FirstOrDefaultAsync();
            return user;
        }
    }
}
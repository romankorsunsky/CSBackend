

using System.Data.Common;
using b1.Models;
using MongoDB.Driver;

namespace b1.Srevices
{
    public class UserRegService
    {
        private readonly IMongoCollection<User> _users;
        public UserRegService(IMongoDatabase mdb)
        {
            _users = mdb.GetCollection<User>("users");
        }

        public async Task<User> CreateUser(UserRegistrationForm u)
        {
            var verified = new User();
            verified.Email = u.Email;
            verified.Fname = u.FirstName;
            verified.Lname = u.LastName;
            verified.Password = u.Password;
            //perform valdiation here:
            await _users.InsertOneAsync(verified);
            return verified;
        }


    }
}

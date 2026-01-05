

using System.Data.Common;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
            bool valid = await isValidUser(u);
            if (!valid)
            {
                throw new Exception("Invalid registration parameters");
            }
            var verified = new User();
            verified.Email = u.Email;
            verified.Fname = u.FirstName;
            verified.Lname = u.LastName;
            string encryptedPwd = null!;
            verified.Password = encryptedPwd;
            await _users.InsertOneAsync(verified);
            return verified;
        }

        private async Task<bool> isValidUser(UserRegistrationForm user)
        {
            if (user == null){
                return false;
            }
            var emailPattern = @"^[a-zA-Z0-9]{6}@"; // didn't finish, its bad.
            bool mtch = Regex.IsMatch(user.Email, emailPattern,RegexOptions.IgnoreCase);
            if (user.Password.Length < 10 || user.Password.Length > 18)
            {
                return false;
            }
            var col = _users;
            var usrs = await col.FindAsync(u => u.Email == user.Email);
            User usrExisting = await usrs.FirstAsync();
            if (usrExisting != null)
            {
                return false;
            }
            return true;
        }

    }
}

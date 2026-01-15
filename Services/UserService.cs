

using System.Data.Common;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using b1.Controllers;
using b1.Models;
using b1.Repositories;
using MongoDB.Driver;

namespace b1.Srevices
{
    public class UserService
    {
        private IUserRepository Users { get; init; } = null!;
        public UserService(IUserRepository userRepo)
        {
            Users = userRepo;
        }

        public async Task<User?> CreateUser(UserRegistrationForm u)
        {
            bool valid = await isValidUser(u);

            if (!valid)
            {
                return null;
            }
            var verified = UserRegistrationForm.CreateUserFromRegistration(u);
            Console.WriteLine($"hashed from retrieved:[{verified.Password}]");
            await Users.AddNewUser(verified);
            return verified;
        }

        private async Task<bool> isValidUser(UserRegistrationForm user)
        {
            if (user is null)
            {
                return false;
            }
            if (!MailAddress.TryCreate(user.Email, out var mail) ||
                    user?.Username.Length < 10 ||
                    user?.Username.Length > 18)
            {
                return false;
            }

            var usr = await Users.GetUserByName(user.Username);

            if (usr is not null)
                return false;
            return true;
        }

        public async Task<User?> FindByName(string name)
        {
            return await Users.GetUserByName(name);
        }
    }
}

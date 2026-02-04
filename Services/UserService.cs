

using System.Data.Common;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using b1.Authentication;
using b1.Controllers;
using b1.Main;
using b1.Models;
using b1.Repositories;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace b1.Srevices
{
    public class UserService
    {
        private UserAuthenticator _userAuth { get; init; } = null!;
        private IMongoCollection<User> _users { get; init; } = null!;
        private IMongoCollection<Portfolio> _portfolios { get; init; } = null!;
        public UserService(IMongoDatabase db, UserAuthenticator authenticator)
        {
            _userAuth = authenticator;
            _users = db.GetCollection<User>("users");
            _portfolios = db.GetCollection<Portfolio>("portfolios"); ;
        }

        public async Task<User?> CreateUser(UserRegistrationForm u)
        {
            bool valid = await isValidUser(u);
            if (!valid)
            {
                return null;
            }
            var verified = UserRegistrationForm.CreateUserFromRegistration(u);
            await _users.InsertOneAsync(verified);
            return verified;
        }

        private async Task<bool> isValidUser(UserRegistrationForm user)
        {
            if (user == null)
            {
                return false;
            }
            //check correct mail. Isn't a substitute for mail verification using a mailing server.
            if (!MailAddress.TryCreate(user.Email, out var mail) ||
                    user?.Username.Length < 10 ||
                    user?.Username.Length > 18)
            {
                return false;
            }
            var usrname = user?.Username;
            var result = await _users.Find(u => u.Username == usrname).FirstOrDefaultAsync(); //if exists already
            if (result != null)
                return false;
            return true;
        }

        public async Task<User?> FindByUserId(string userId)
        {
            return await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        }
        
        public async Task<ProfileInfo?> GetProfile(ClaimsPrincipal principal)
        {
            var usernameClaim = principal.FindFirst("sub");
            if (usernameClaim == null)
                return null;
            var username = usernameClaim.Value;
            var usr = await FindByUserId(username);
            if (usr == null)
                return null;
            var prof = new ProfileInfo()
            {
                Email = usr.Email,
                FirstName = usr.Fname,
                LastName = usr.Lname
            };
            return prof;
        }
        public async Task<TokenTriplet> AuthenticateUser(AuthRequest req)
        {
            var res = await _userAuth.Handle(req);
            return res;
        }

        public async Task GetUserMonthlyReport()
        {
            //todo, gather some stats and send to the user.
        }
    }
}



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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ScottPlot.Reporting;

namespace b1.Srevices
{
    public class UserService
    {
        private UserAuthenticator _userAuth { get; init; } = null!;
        private IUserRepository _userRepo { get; set; }
        public UserService(IUserRepository usrRepo,
            UserAuthenticator authenticator)
        {
            _userRepo = usrRepo;
            _userAuth = authenticator;
        }

        public async Task<User?> CreateUser(UserRegistrationForm u)
        {
            bool valid = await isValidUser(u);
            if (!valid)
            {
                return null;
            }
            var verified = UserRegistrationForm.CreateUserFromRegistration(u);
            await _userRepo.AddNewUser(verified);
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
            if (usrname == null)
            {
                return false;
            }
            //var result = await _users.Find(u => u.Username == usrname).FirstOrDefaultAsync(); //if exists already
            var result = await _userRepo.GetUserByUsername(usrname);
            if (result != null)
                return false;
            return true;
        }

        public async Task<User?> FindByUserId(string userId)
        {
            //return await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            return await _userRepo.GetUserById(userId);
        }
        
        public async Task<ProfileInfoDTO?> GetProfile(ClaimsPrincipal principal)
        {
            var usernameClaim = principal.FindFirst("sub");
            if (usernameClaim == null)
                return null;
            var username = usernameClaim.Value;
            var usr = await FindByUserId(username);
            if (usr == null)
                return null;
            var prof = new ProfileInfoDTO(usr.Fname, usr.Lname, usr.Email, usr.Balance);
            return prof;
        }
        public async Task<TokenTriplet> AuthenticateUser(AuthRequest req)
        {
            var res = await _userAuth.Handle(req);
            return res;
        }
    }
}

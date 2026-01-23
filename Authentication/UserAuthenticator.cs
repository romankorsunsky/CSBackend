

using b1.Models;
using b1.Srevices;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using BC = BCrypt.Net.BCrypt;
namespace b1.Authentication
{
    public class UserAuthenticator
    {
        private IConfiguration Cfg { get; init; }
        private UserService UsrService { get; init; }
        private TokenProvider JTProvider { get; set; }
        public UserAuthenticator(IConfiguration config, UserService userService, TokenProvider tokenProvider)
        {
            Cfg = config;
            UsrService = userService;
            JTProvider = tokenProvider;
        }
        public async Task<TokenTriplet> Handle(AuthRequest req)
        {
            var user = await UsrService.FindByName(req.Username);
            if (user is null)
            {
                throw new Exception("Bad auth request or user doesn't exist");
            }
            if (!BC.Verify(req.Password, user.Password,true))
            {
                Console.WriteLine($"password in auth request = [{req.Password}], hashed = [{user.Password}]");
                throw new Exception("Bad password");
            }
            var token = JTProvider.CreateToken(user);
            TokenTriplet triplet = new TokenTriplet()
            {
                AccessToken = token,
                IdToken = "dummy",
                RefreshToken = "dummy"
            };
            Console.WriteLine("Handle issued AccessToken: " + token);
            return triplet;
        }
    }
    public struct AuthRequest
        {
            public string Username { get; init; }
            public string Password { get; init; }
        }
}
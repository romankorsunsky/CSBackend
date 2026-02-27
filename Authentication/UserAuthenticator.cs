using b1.Models;
using b1.Repositories;
using b1.Srevices;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using BC = BCrypt.Net.BCrypt;
namespace b1.Authentication
{
    public class UserAuthenticator
    {
        private IUserRepository _users { get; init; }
        private TokenProvider _tokenProvider { get; set; }
        public UserAuthenticator(TokenProvider tokenProvider,IUserRepository userRepo)
        {
            _users = userRepo;
            _tokenProvider = tokenProvider;
        }
        public async Task<TokenTriplet> Handle(AuthRequest req)
        {
            var user = await _users.GetUserByUsername(req.Username);
            if (user is null)
            {
                throw new Exception("Bad auth request or user doesn't exist");
            }
            if (!BC.Verify(req.Password, user.Password,true))
            {
                throw new Exception("Bad password");
            }
            var token = _tokenProvider.CreateToken(user);
            TokenTriplet triplet = new TokenTriplet()
            {
                AccessToken = token,
                IdToken = "dummy",
                RefreshToken = "dummy"
            };
            return triplet;
        }
    }
    public struct AuthRequest
        {
            public string Username { get; init; }
            public string Password { get; init; }
        }
}
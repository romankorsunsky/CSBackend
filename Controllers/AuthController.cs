

using System.Security.Claims;
using b1.Authentication;
using b1.Models;
using b1.Srevices;
using DnsClient.Protocol;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BC = BCrypt.Net.BCrypt;
namespace b1.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/users")]
    public class AuthController : ControllerBase
    {

        private readonly UserService _usrService;
        private readonly UserAuthenticator _userAuth;
        public AuthController(UserService userv, UserAuthenticator authenticator)
        {
            _usrService = userv;
            _userAuth = authenticator;
        }
        [Authorize]
        [HttpGet]
        [Route("profile/{username}")]
        public async Task<ActionResult<ProfileInfo>> GetProfile([FromRoute] string username)
        {
            Console.WriteLine("requestiing profile");
            var clm = User.FindFirst("sub")?.Value;
            var usr = await _usrService.FindByName(username);
            if (clm == null || usr == null)
            {
                //if somehow the attached token had no 'sub' claim and we are past [Authorized], we got a problem
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            if (clm != usr.Username)
            {
                return StatusCode(StatusCodes.Status401Unauthorized); //can't ask for other people's profiles (unless you are some moderator or admin dunno)
            }
            var prof = new ProfileInfo()
            {
                Email = usr.Email,
                FirstName = usr.Fname,
                LastName = usr.Lname
            };
            Console.WriteLine($"sending Profile(${prof.Email},${prof.FirstName},${prof.LastName})");
            return Ok(prof);
        }
        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<TokenTriplet>> Login([FromBody] AuthRequest req)
        {
            var res = await _userAuth.Handle(req);
            Console.WriteLine("AuthRequest={Username:" + req.Username + ",Password:" + req.Password);
            if (res == null)
            {
                Console.WriteLine("res is NULL");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }
            return Ok(res);
        }

        [HttpPost]
        [Route("register")]
        public async Task<ActionResult> CreateUser([FromBody] UserRegistrationForm u)
        {
            var usr = await _usrService.CreateUser(u);
            if (usr == null)
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity);
            }
            return StatusCode(StatusCodes.Status201Created);
        }
    }
}

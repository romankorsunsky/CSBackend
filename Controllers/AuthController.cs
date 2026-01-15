

using System.Security.Claims;
using b1.Authentication;
using b1.Models;
using b1.Srevices;
using DnsClient.Protocol;
using Microsoft.AspNetCore.Authentication;
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
        [HttpGet]
        public ActionResult GetOk()
        {
            return Ok();
        }
        [HttpPost]
        [Route("login")]
        public async Task<string> Login([FromBody] AuthRequest req)
        {
            var res = await _userAuth.Handle(req);
            return res;
        }

        [HttpPost]
        [Route("register")]
        public async Task<ActionResult> CreateUser([FromBody] UserRegistrationForm u)
        {
            Console.WriteLine($"Created with plaintext:{u.Password}");
            var usr = await _usrService.CreateUser(u);
            if (usr == null)
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity);
            }
            return StatusCode(StatusCodes.Status201Created);
        }
    }
}

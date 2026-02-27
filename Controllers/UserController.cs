

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
    public class UserController : ControllerBase
    {

        private readonly UserService _usrService;
        public UserController(UserService userv)
        {
            _usrService = userv;
        }
        
        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<TokenTriplet>> Login([FromBody] AuthRequest req)
        {
            var res = await _usrService.AuthenticateUser(req);
            if (res == null)
            {
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
            return Created();
        }

        [Authorize]
        [HttpGet]
        [Route("profile")]
        public async Task<ActionResult<ProfileInfoDTO>> GetProfile()
        {
            var principal = User;
            var prof = await _usrService.GetProfile(principal);
            if (prof != null)
                return Ok(prof);
            return NotFound();
        }
    }
}

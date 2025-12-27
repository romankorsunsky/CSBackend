

using b1.Models;
using b1.Srevices;
using Microsoft.AspNetCore.Mvc;

namespace b1.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/users")]
    public class UsersController : ControllerBase
    {

        private readonly UserRegService _usrService;

        public UsersController(UserRegService userv)
        {
            _usrService = userv;
        }
        [HttpGet]
        public ActionResult GetOk()
        {
            return Ok();
        }
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser([FromBody] UserRegistrationForm u)
        {
            var usr = await _usrService.CreateUser(u);
            return Ok(usr);
        }
    }
}
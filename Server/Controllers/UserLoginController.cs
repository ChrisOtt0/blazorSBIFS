using blazorSBIFS.Server.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace blazorSBIFS.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserLoginController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IUserService _userService;

        public UserLoginController(DataContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        // Test method for reading claims
        [HttpGet, Authorize(Roles = "admin")]
        public ActionResult<object> GetMe()
        {
            var userID = _userService.GetUserID();
            return Ok(new { userID });

            //var userID = User?.Identity?.Name;
            //var userRole = User?.FindFirstValue(ClaimTypes.Role);
            //return Ok(new {userID, userRole});
        }

        [HttpPost("Register")]
        public async Task<ActionResult> Register(UserLoginDto request)
        {
            var user = await _context.UserLogins.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user != null)
                return UnprocessableEntity("Email not valid or taken.");

            string salt = new SaltAdapter().GetSalt();
            string hashedPass = SecurityTools.HashString(request.Password, salt);

            user = await _context.Users.FirstOrDefaultAsync(u => u.Password == hashedPass);
            if (user != null)
                return UnprocessableEntity("Password not valid.");

            User u = new User();

            await _context.Users.AddAsync(u);
            await _context.SaveChangesAsync();

            string token = JwtTools.CreateToken(u);
            return NoContent();
        }

        [HttpPost("Login")]
        public async Task<ActionResult<object>> Login(UserLoginDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return Forbid("Wrong username or password.");

            string salt = new SaltAdapter().GetSalt();
            string hashedPass = SecurityTools.HashString(request.Password, salt);

            if (user.Password != hashedPass)
                return Forbid("Wrong username or password.");

            var jwt = JwtTools.CreateToken(user);
            return Ok(new { jwt });
        }
    }
}

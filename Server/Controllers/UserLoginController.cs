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

        // Change to accomodate change in Models (User <-> UserLogin)
        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<ActionResult> Register(UserRegisterDto request)
        {
            var user = await _context.UserLogins.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user != null)
                return UnprocessableEntity("Email not valid or taken.");

            string salt = new SaltAdapter().GetSalt();
            string hashedPass = SecurityTools.HashString(request.Password, salt);

            user = await _context.UserLogins.FirstOrDefaultAsync(u => u.Password == hashedPass);
            if (user != null)
                return UnprocessableEntity("Password not valid.");

            User u = new User();
            u.Name = request.Name;
            
            UserLogin ul = new UserLogin();
            ul.Email = request.Email;
            ul.Password = hashedPass;
            ul.User = u;

            await _context.Users.AddAsync(u);
            await _context.UserLogins.AddAsync(ul);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Change to accomodate change in Models (User <-> UserLogin)
        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<ActionResult<object>> Login(UserLoginDto request)
        {
            var login = await _context.UserLogins.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (login == null)
                return Unauthorized("Wrong username or password.");

            string salt = new SaltAdapter().GetSalt();
            string hashedPass = SecurityTools.HashString(request.Password, salt);

            if (login.Password != hashedPass)
                return Unauthorized("Wrong username or password.");

            var user = await _context.Users.FindAsync(login.UserID);
            if (user == null)
                return BadRequest("No such user. Please contact an administrator.");

            var jwt = JwtTools.CreateToken(user);
            return Ok(new { jwt });
        }
    }
}

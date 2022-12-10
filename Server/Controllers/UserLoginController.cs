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

        public UserLoginController(DataContext context)
        {
            _context = context;
        }

        /*
         *  Users must register to use the service. Users are autmatically set up with user privileges.
         *  Admin rights are given manually, by making a manual change in the database, to prevent users from accidentally gaining admin rights.
         */
        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<ActionResult<TokenDto>> Register(UserRegisterDto request)
        {
            UserLogin? user = await _context.UserLogins.FirstOrDefaultAsync(u => u.Email == request.Email);
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

            string jwt = JwtTools.CreateToken(u);
            return Ok(new TokenDto { Jwt = jwt });
        }

        // User must be logged in to use the system. 
        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<ActionResult<TokenDto>> Login(UserLoginDto request)
        {
            UserLogin? login = await _context.UserLogins.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (login == null)
                return Unauthorized("Wrong username or password.");
            
            string salt = new SaltAdapter().GetSalt();
            string hashedPass = SecurityTools.HashString(request.Password, salt);

            if (login.Password != hashedPass)
                return Unauthorized("Wrong username or password.");

            User? user = await _context.Users.FindAsync(login.UserID);
            if (user == null)
                return BadRequest("No such user. Please contact an administrator.");

            string jwt = JwtTools.CreateToken(user);
            return Ok(new TokenDto { Jwt = jwt });
            
        }
    }
}

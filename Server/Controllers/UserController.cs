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
    public class UserController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IUserService _userService;

        public UserController(DataContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        [HttpGet("Read"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult<UserDto>> Get()
        {
            var userID = _userService.GetUserID();
            var login = await _context.UserLogins
                .Where(l => l.UserID == userID)
                .Include(l => l.User)
                .FirstAsync();

            IJson data = new UserDto
            {
                Name = login.User.Name,
                Email = login.Email
            };

            return Ok(data);
        }

        [HttpPut("UpdateUser"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult> Update(UserDto request)
        {
            string hashedPass = string.Empty;
            int userID = _userService.GetUserID();
            var login = await _context.UserLogins
                .Where(l => l.UserID == userID)
                .Include(l => l.User)
                .FirstAsync();
            if (login == null)
                return BadRequest("No such user.");

            if (request.Name != null && request.Name != string.Empty && request.Name != login.User.Name)
            {
                login.User.Name = request.Name;
            }
            if (request.Email != null && request.Email != string.Empty && request.Email != login.Email)
            {
                var isUser = await _context.UserLogins.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (isUser != null)
                    return UnprocessableEntity("User with given email already exists. Could not update the account.");

                login.Email = request.Email;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("UpdatePassword"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult> Update(PasswordDto request)
        {
            int userID = _userService.GetUserID();
            var user = await _context.Users.FindAsync(userID);
            if (user == null)
                return BadRequest("No such user.");

            var login = await _context.UserLogins
                .Where(l => l.UserID == userID)
                .FirstAsync();

            string salt = new SaltAdapter().GetSalt();
            string hashedPass = SecurityTools.HashString(request.OldPassword, salt);

            if (login.Password != hashedPass)
                return UnprocessableEntity("Inaccurate password.");

            string newHashedPass = SecurityTools.HashString(request.NewPassword, salt);
            login.Password = newHashedPass;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("Delete"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult> Delete(UserLoginDto request)
        {
            int userID = _userService.GetUserID();
            var login = await _context.UserLogins
                .Where(l => l.UserID == userID)
                .Include(l => l.User)
                .FirstAsync();
            if (login == null)
                return BadRequest("No such user.");

            if (login.Email != request.Email)
                return UnprocessableEntity("Wrong email or password.");

            string salt = new SaltAdapter().GetSalt();
            string hashedPass = SecurityTools.HashString(request.Password, salt);

            if (login.Password != hashedPass)
                return UnprocessableEntity("Wrong email or password.");

            var user = login.User;
            List<Group> groups = await _context.Groups.Where(g => g.OwnerID == userID).ToListAsync();

            _context.Groups.RemoveRange(groups);
            _context.Users.Remove(user);
            _context.UserLogins.Remove(login);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

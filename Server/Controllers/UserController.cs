using blazorSBIFS.Server.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace blazorSBIFS.Server.Controllers
{
    // Returns custom DTOs instead of User objects, to be able to include user email from the UserLogin table for potential manipulation.
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

        [HttpGet("ReadOne"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult<UserDto>> ReadOne()
        {
            int userID = _userService.GetUserID();
            UserLogin? login = await _context.UserLogins
                .Where(l => l.UserID == userID)
                .Include(l => l.User)
                .FirstOrDefaultAsync();
            if (login == null)
                return BadRequest("No such user.");

            IJson data = new UserDto
            {
                UserID = userID,
                Name = login.User.Name,
                Email = login.Email
            };

            return Ok(data);
        }

        [HttpGet("ReadMany"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult<List<UserDto>>> ReadMany()
        {
            List<UserLogin> logins = await _context.UserLogins
                .Include(l => l.User)
                .ToListAsync();

            List<UserDto> users = new List<UserDto>();
            foreach (UserLogin login in logins)
            {
                users.Add(new UserDto
                {
                    UserID = login.User.UserID,
                    Name = login.User.Name,
                    Email = login.Email
                });
            }

            return Ok(users);
        }

        [HttpPut("UpdateUser"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult> Update(UserDto request)
        {
            string hashedPass = string.Empty;
            int userID = _userService.GetUserID();
            UserLogin? login = await _context.UserLogins
                .Where(l => l.UserID == userID)
                .Include(l => l.User)
                .FirstOrDefaultAsync();
            if (login == null)
                return BadRequest("No such user.");

            if (request.Name != null && request.Name != string.Empty && request.Name != login.User.Name)
            {
                login.User.Name = request.Name;
            }
            if (request.Email != null && request.Email != string.Empty && request.Email != login.Email)
            {
                UserLogin? isUser = await _context.UserLogins.FirstOrDefaultAsync(u => u.Email == request.Email);
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
            User? user = await _context.Users.FindAsync(userID);
            if (user == null)
                return BadRequest("No such user.");

            UserLogin? login = await _context.UserLogins
                .Where(l => l.UserID == userID)
                .FirstOrDefaultAsync();
            if (login == null)
                return BadRequest("No such user.");

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
            UserLogin? login = await _context.UserLogins
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

            User user = login.User;
            List<Group> groups = await _context.Groups.Where(g => g.OwnerID == userID).ToListAsync();

            _context.Groups.RemoveRange(groups);
            _context.Users.Remove(user);
            _context.UserLogins.Remove(login);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

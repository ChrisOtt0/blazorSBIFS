using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace blazorSBIFS.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminGroupController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IUserService _userService;

        public AdminGroupController(DataContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        [HttpGet("ReadOne"), Authorize(Roles = "admin")]
        public async Task<ActionResult<Group>> Get(GroupDto requested)
        {
            var group = await _context.Groups
                .Where(g => g.GroupID == requested.GroupID)
                .Include(g => g.Participants)
                .FirstAsync();

            if (group == null)
                return BadRequest("No such group.");

            return Ok(group);
        }

        [HttpGet("ReadMany"), Authorize(Roles = "admin")]
        public async Task<ActionResult<List<Group>>> Get(EmailDto request)
        {
            var login = await _context.UserLogins
                .Where(l => l.Email == request.Email)
                .FirstAsync();
            if (login == null)
                return BadRequest("No such user.");

            var groups = await _context.Groups
                .Where(g => g.OwnerID == login.UserID)
                .Include (g => g.Participants)
                .ToListAsync();
            return Ok(groups);
        }

        [HttpPost("Create"), Authorize(Roles = "admin")]
        public async Task<ActionResult<List<Group>>> Create(EmailDto request)
        {
            var login = await _context.UserLogins
                .Where(l => l.Email == request.Email)
                .FirstAsync();
            if (login == null)
                return BadRequest("No such user.");

            var user = await _context.Users.FindAsync(login.UserID);
            if (user == null)
                return BadRequest("No such user.");

            Group group = new Group();
            group.OwnerID = user.UserID;
            group.Name = "New Group";
            user.Groups.Add(group);
            group.Participants.Add(user);

            await _context.Groups.AddAsync(group);
            await _context.SaveChangesAsync();

            // Necessity for a group name which is returned instead? 
            List<Group> groups = await _context.Groups.Where(g => g.OwnerID == user.UserID).ToListAsync();

            return new ObjectResult(groups) { StatusCode = StatusCodes.Status201Created };
        }

        [HttpDelete("Delete"), Authorize(Roles = "admin")]
        public async Task<ActionResult> Delete(GroupDto request)
        {
            Group? group = await _context.Groups.FindAsync(request.GroupID);
            if (group == null)
                return BadRequest("No such group.");

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

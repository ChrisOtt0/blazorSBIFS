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

        [HttpPut("UpdateName"), Authorize(Roles = "admin")]
        public async Task<ActionResult<Group>> UpdateName(GroupNameDto request)
        {
            if (request.Name == null || request.Name == "")
                return BadRequest("Request incomplete.");

            var group = await _context.Groups
                .Where(g => g.GroupID == request.GroupID)
                .Include(g => g.Participants)
                .FirstOrDefaultAsync();
            if (group == null)
                return BadRequest("No such group.");

            group.Name = request.Name;
            await _context.SaveChangesAsync();
            return Ok(group);
        }

        [HttpPut("AddParticipant"), Authorize(Roles = "admin")]
        public async Task<ActionResult<Group>> AddParticipant(GroupUserDto request)
        {
            if (request.GroupRequest == null || request.ParticipantRequest == null)
                return BadRequest("Request incomplete.");

            int userID = _userService.GetUserID();
            var group = await _context.Groups
                .Where(g => g.GroupID == request.GroupRequest.GroupID)
                .Include(g => g.Participants)
                .FirstOrDefaultAsync();
            if (group == null)
                return BadRequest("No such group.");

            var participant = await _context.UserLogins
                .Where(u => u.Email == request.ParticipantRequest.Email)
                .Include(u => u.User)
                .FirstOrDefaultAsync();
            if (participant == null)
                return BadRequest("No such user");

            if (group.Participants.Contains(participant.User))
                return BadRequest("User is already a participant.");

            group.Participants.Add(participant.User);
            await _context.SaveChangesAsync();

            return Ok(group);
        }

        [HttpPut("RemoveParticipant"), Authorize(Roles = "admin")]
        public async Task<ActionResult<Group>> RemoveParticipant(GroupUserDto request)
        {
            if (request.GroupRequest == null || request.ParticipantRequest == null)
                return BadRequest("Request incomplete.");

            int userID = _userService.GetUserID();
            var group = await _context.Groups
                .Where(g => g.GroupID == request.GroupRequest.GroupID)
                .Include(g => g.Participants)
                .FirstOrDefaultAsync();
            if (group == null)
                return BadRequest("No such group.");

            var participant = await _context.UserLogins
                .Where(u => u.Email == request.ParticipantRequest.Email)
                .Include(u => u.User)
                .FirstOrDefaultAsync();
            if (participant == null)
                return BadRequest("No such user");

            if (!group.Participants.Contains(participant.User))
                return BadRequest("User is not a participant in the selected group.");

            group.Participants.Remove(participant.User);
            await _context.SaveChangesAsync();

            return Ok(group);
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

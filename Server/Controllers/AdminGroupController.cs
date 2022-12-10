using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace blazorSBIFS.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminGroupController : ControllerBase
    {
        private readonly DataContext _context;

        public AdminGroupController(DataContext context)
        {
            _context = context;
        }

        [HttpPost("ReadMany"), Authorize(Roles = "admin")]
        public async Task<ActionResult<List<Group>>> Get(EmailDto request)
        {
            User? user = await _context.UserLogins
                .Where(l => l.Email == request.Email)
                .Select(l => l.User)
                .FirstOrDefaultAsync();
            if (user == null)
                return BadRequest("No such user.");

            List<Group> groups = await _context.Groups
                .Where(g => g.OwnerID == user.UserID)
                .ToListAsync();
            return Ok(groups);
        }

        [HttpPost("Create"), Authorize(Roles = "admin")]
        public async Task<ActionResult<List<Group>>> Create(EmailDto request)
        {
            User? user = await _context.UserLogins
                .Where(l => l.Email == request.Email)
                .Select(l => l.User)
                .FirstAsync();
            if (user == null)
                return BadRequest("No such user.");

            Group group = new Group();
            group.Name = "New Group";
            group.OwnerID = user.UserID;
            var entry = _context.Entry(user);
            entry.Entity.Groups.Add(group);
            group.Participants.Add(user);

            await _context.Groups.AddAsync(group);
            await _context.SaveChangesAsync();

            // 201 with normal ActionResult resulted in weird error, this works
            return new ObjectResult(user.Groups) { StatusCode = StatusCodes.Status201Created };
        }

        [HttpPut("UpdateName"), Authorize(Roles = "admin")]
        public async Task<ActionResult<Group>> UpdateName(GroupNameDto request)
        {
            if (request.Name == null || request.Name == "")
                return BadRequest("Request incomplete.");

            Group? group = await _context.Groups
                .Where(g => g.GroupID == request.GroupID)
                .Include(g => g.Participants)
                .Include(g => g.Activities)
                .FirstOrDefaultAsync();
            if (group == null)
                return BadRequest("No such group.");

            group.Name = request.Name;
            await _context.SaveChangesAsync();
            return Ok(group);
        }

        [HttpPut("AddParticipant"), Authorize(Roles = "admin")]
        public async Task<ActionResult<Group>> AddParticipant(GroupEmailDto request)
        {
            if (request.GroupRequest == null || request.EmailRequest == null)
                return BadRequest("Request incomplete.");

            Group? group = await _context.Groups
                .Where(g => g.GroupID == request.GroupRequest.GroupID)
                .Include(g => g.Participants)
                .Include(g => g.Activities)
                .FirstOrDefaultAsync();
            if (group == null)
                return BadRequest("No such group.");

            User? participant = await _context.UserLogins
                .Where(u => u.Email == request.EmailRequest.Email)
                .Select(u => u.User)
                .FirstOrDefaultAsync();
            if (participant == null)
                return BadRequest("No such user");

            if (group.Participants.Contains(participant))
                return BadRequest("User is already a participant.");

            group.Participants.Add(participant);
            await _context.SaveChangesAsync();

            return Ok(group);
        }

        [HttpPut("RemoveParticipant"), Authorize(Roles = "admin")]
        public async Task<ActionResult<Group>> RemoveParticipant(GroupUserDto request)
        {
            if (request.GroupRequest == null || request.UserRequest == null)
                return BadRequest("Request incomplete.");

            Group? group = await _context.Groups
                .Where(g => g.GroupID == request.GroupRequest.GroupID)
                .Include(g => g.Participants)
                .Include(g => g.Activities)
                .FirstOrDefaultAsync();
            if (group == null)
                return BadRequest("No such group.");

            User? participant = await _context.UserLogins
                .Where(u => u.UserID == request.UserRequest.UserID)
                .Select(u => u.User)
                .FirstOrDefaultAsync();
            if (participant == null)
                return BadRequest("No such user");

            if (participant.UserID == group.OwnerID)
                return Unauthorized("Cannot remove the owner of the group.");

            if (!group.Participants.Contains(participant))
                return BadRequest("User is not a participant in the selected group.");

            group.Participants.Remove(participant);
            await _context.SaveChangesAsync();

            return Ok(group);
        }

        [HttpDelete("Delete"), Authorize(Roles = "admin")]
        public async Task<ActionResult> Delete(GroupDto request)
        {
            Group? group = await _context.Groups.FindAsync(request.GroupID);
            if (group == null)
                return BadRequest("No such group.");

            List<Activity>? activities = await _context.Activities
                .Where(a => a.Group == group).ToListAsync();
            if (activities != null && activities.Any())
            {
                foreach (Activity activity in activities)
                {
                    _context.Activities.Remove(activity);
                }
            }
            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

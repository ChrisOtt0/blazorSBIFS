using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace blazorSBIFS.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IUserService _userService;

        public GroupController(DataContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        [HttpPost("ReadOne"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult<Group>> ReadOne(GroupDto requested)
        {
            var group = await _context.Groups
                .Where(g => g.GroupID == requested.GroupID)
                .Include(g => g.Participants)
                .Include(g => g.Activities)
                .FirstAsync();
            if (group == null)
                return BadRequest("No such group.");

            return Ok(group);
        }

        [HttpGet("ReadMany"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult<List<Group>>> ReadMany()
        {
            int userID = _userService.GetUserID();
            // Necessity for a group name which is returned instead? 
            List<Group> groups = await _context.Groups
                .Where(g => g.OwnerID == userID)
                .Include(g => g.Participants)
                .Include(g => g.Activities)
                .ToListAsync();
            return Ok(groups);
        }

        [HttpPost("ReadParticipants"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult<List<User>>> ReadParticipants(GroupDto request)
        {
            var participants = await _context.Groups
                .Where(g => g.GroupID == request.GroupID)
                .Select(g => g.Participants)
                .ToListAsync();

            return Ok(participants);
        }

        [HttpPost("Create"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult<List<Group>>> Create()
        {
            int userID = _userService.GetUserID();
            var user = await _context.Users.FindAsync(userID);
            if (user == null)
                return BadRequest("No such user.");

            Group group = new Group();
            group.Name = "New Group";
            user.Groups.Add(group);
            group.Participants.Add(user);
            group.OwnerID = userID;

            await _context.Groups.AddAsync(group);

            await _context.SaveChangesAsync();

            // Necessity for a group name which is returned instead? 
            List<Group> groups = await _context.Groups.Where(g => g.OwnerID == userID).ToListAsync();

            return new ObjectResult(groups) { StatusCode = StatusCodes.Status201Created };
        }

        [HttpPut("UpdateGroup"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult> UpdateGroup(Group request)
        {
            var group = await _context.Groups.
                Where(g => g.GroupID == request.GroupID)
                .Include(g => g.Participants)
                .Include(g => g.Activities)
                .FirstOrDefaultAsync();
            if (group == null)
                return BadRequest("No such group");

            group = request;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("UpdateName"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult<Group>> UpdateName(GroupNameDto request)
        {
            if (request.Name == null || request.Name == "")
                return BadRequest("Request incomplete.");

            int userID = _userService.GetUserID();
            var group = await _context.Groups
                .Where(g => g.GroupID == request.GroupID)
                .Include(g => g.Participants)
                .FirstOrDefaultAsync();
            if (group == null)
                return BadRequest("No such group.");

            if (group.OwnerID != userID)
                return Unauthorized("Only the group owner can update the group name.");

            group.Name = request.Name;
            await _context.SaveChangesAsync();
            return Ok(group);
        }

        [HttpPut("AddParticipant"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult<Group>> AddParticipant(GroupUserDto request)
        {
            if (request.GroupRequest == null || request.UserRequest == null)
                return BadRequest("Request incomplete.");

            int userID = _userService.GetUserID();
            var group = await _context.Groups
                .Where(g => g.GroupID == request.GroupRequest.GroupID)
                .Include(g => g.Participants)
                .FirstOrDefaultAsync();
            if (group == null)
                return BadRequest("No such group.");

            if (group.OwnerID != userID)
                return Unauthorized("Only the group owner can invite participants.");

            var participant = await _context.UserLogins
                .Where(u => u.Email == request.UserRequest.Email)
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

        [HttpPut("RemoveParticipant"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult<Group>> RemoveParticipant(GroupUserDto request)
        {
            if (request.GroupRequest == null || request.UserRequest == null)
                return BadRequest("Request incomplete.");

            int userID = _userService.GetUserID();
            var group = await _context.Groups
                .Where(g => g.GroupID == request.GroupRequest.GroupID)
                .Include(g => g.Participants)
                .FirstOrDefaultAsync();
            if (group == null)
                return BadRequest("No such group.");

            if (group.OwnerID != userID)
                return Unauthorized("Only the group owner can remove participants.");

            var participant = await _context.UserLogins
                .Where(u => u.Email == request.UserRequest.Email)
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

        [HttpDelete("Delete"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult> Delete(GroupDto requested)
        {
            int userID = _userService.GetUserID();
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.GroupID == requested.GroupID && g.OwnerID == userID);
            if (group == null)
                return BadRequest("No such group.");

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

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
            Group? group = await _context.Groups
                .Where(g => g.GroupID == requested.GroupID)
                .Include(g => g.Participants)
                .Include(g => g.Activities)
                .FirstOrDefaultAsync();
            if (group == null)
                return BadRequest("No such group.");

            return Ok(group);
        }

        [HttpGet("ReadMany"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult<List<Group>>> ReadMany()
        {
            int userID = _userService.GetUserID();
            List<Group> groups = await _context.Groups
                .Where(g => g.OwnerID == userID)
                .ToListAsync();
            return Ok(groups);
        }

        [HttpPost("IsOwner"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult<bool>> IsOwner(GroupDto request)
        {
            int userID = _userService.GetUserID();
            Group? group = await _context.Groups
                .Where(g => g.GroupID == request.GroupID)
                .FirstOrDefaultAsync();
            if (group == null)
                return BadRequest("No such group");

            return Ok(userID == group.OwnerID);
        }

        [HttpPost("ReadParticipants"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult<List<User>>> ReadParticipants(GroupDto request)
        {
            List<User>? participants = await _context.Groups
                .Where(g => g.GroupID == request.GroupID)
                .Select(g => g.Participants)
                .FirstOrDefaultAsync();
            if (participants == null)
                return BadRequest("No such group.");

            return Ok(participants);
        }

        [HttpPost("Create"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult<List<Group>>> Create()
        {
            int userID = _userService.GetUserID();
            User? user = await _context.Users.
                Where(u => u.UserID == userID)
                .Include(u => u.Groups)
                .FirstOrDefaultAsync();
            if (user == null)
                return BadRequest("No such user.");

            Group group = new Group();
            group.Name = "New Group";
            group.OwnerID = userID;
            user.Groups.Add(group);
            group.Participants.Add(user);

            await _context.Groups.AddAsync(group);
            await _context.SaveChangesAsync();

            // 201 with normal ActionResult resulted in weird error, this works
            return new ObjectResult(user.Groups) { StatusCode = StatusCodes.Status201Created };
        }

        [HttpPut("UpdateGroup"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult> UpdateGroup(Group request)
        {
            Group? group = await _context.Groups.
                Where(g => g.GroupID == request.GroupID)
                .Include(g => g.Participants)
                .Include(g => g.Activities)
                .FirstOrDefaultAsync();
            if (group == null)
                return BadRequest("No such group");

            // Updates all scalary information.
            _context.Entry(group).CurrentValues.SetValues(request);

            // The naming hell below, stems from how EF updates relationships.
            // However ugly it does ensure only relationships are updated.
            // Update Participants
            List<User> users = group.Participants.ToList();
            foreach (User user in users)
            {
                User? participant = request.Participants.SingleOrDefault(r => r.UserID == user.UserID);
                if (participant == null)
                    group.Participants.Remove(user);
            }

            foreach (User participant in request.Participants)
            {
                User? user = group.Participants.SingleOrDefault(u => u.UserID == participant.UserID);
                if (user == null)
                    group.Participants.Add(participant);
            }

            // Update Activities
            List<Activity> activities = group.Activities.ToList();
            foreach (Activity a in activities)
            {
                Activity? activity = request.Activities.SingleOrDefault(r => r.ActivityID == a.ActivityID);
                if (activity == null)
                    group.Activities.Remove(a);
            }

            foreach (Activity activity in request.Activities)
            {
                Activity? a = group.Activities.SingleOrDefault(g => g.ActivityID == activity.ActivityID);
                if (a == null)
                    group.Activities.Add(activity);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("UpdateName"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult<Group>> UpdateName(GroupNameDto request)
        {
            if (request.Name == null || request.Name == "")
                return BadRequest("Request incomplete.");

            int userID = _userService.GetUserID();
            Group? group = await _context.Groups
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
        public async Task<ActionResult<Group>> AddParticipant(GroupEmailDto request)
        {
            if (request.GroupRequest == null || request.EmailRequest == null)
                return BadRequest("Request incomplete.");

            int userID = _userService.GetUserID();
            Group? group = await _context.Groups
                .Where(g => g.GroupID == request.GroupRequest.GroupID)
                .Include(g => g.Participants)
                .Include(g => g.Activities)
                .FirstOrDefaultAsync();
            if (group == null)
                return BadRequest("No such group.");

            if (group.OwnerID != userID)
                return Unauthorized("Only the group owner can invite participants.");

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

        [HttpPut("RemoveParticipant"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult<Group>> RemoveParticipant(GroupUserDto request)
        {
            if (request.GroupRequest == null || request.UserRequest == null)
                return BadRequest("Request incomplete.");

            int userID = _userService.GetUserID();
            Group? group = await _context.Groups
                .Where(g => g.GroupID == request.GroupRequest.GroupID)
                .Include(g => g.Participants)
                .Include(g => g.Activities)
                .FirstOrDefaultAsync();
            if (group == null)
                return BadRequest("No such group.");

            if (group.OwnerID != userID)
                return Unauthorized("Only the group owner can remove participants.");

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

        [HttpDelete("Delete"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult> Delete(GroupDto requested)
        {
            int userID = _userService.GetUserID();
            Group? group = await _context.Groups.FirstOrDefaultAsync(g => g.GroupID == requested.GroupID && g.OwnerID == userID);
            if (group == null)
                return BadRequest("No such group.");

            if (group.OwnerID != userID)
                return Unauthorized("Only the group owner can delete the group");

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpPut("LeaveGroup"), Authorize(Roles = "admin, user")] //Leave group as participant (not owner)
        public async Task<ActionResult> LeaveGroup(GroupDto requested)
        {
            int userID = _userService.GetUserID();
            Group? group = await _context.Groups
                .Where(g => g.GroupID == requested.GroupID)
                .Include(g => g.Participants)
                .FirstOrDefaultAsync();
            if (group == null)
                return BadRequest("No such group.");

            if (group.OwnerID == userID)
                return Unauthorized("The owner of the group cannot leave the group.");

            User? user = await _context.UserLogins
                .Where(u => u.UserID == userID)
                .Select(u => u.User)
                .FirstOrDefaultAsync();
            if (user == null)
                return BadRequest("No such user.");

            if (!group.Participants.Contains(user))
                return BadRequest("User is not a participant in the selected group.");

            group.Participants.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
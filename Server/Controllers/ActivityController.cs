using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace blazorSBIFS.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IUserService _userService;

        public ActivityController(DataContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        [HttpPost("Create"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult<List<Activity>>> Create(GroupDto request)
        {
            int userID = _userService.GetUserID();
            User? user = await _context.Users.FindAsync(userID);
            if (user == null)
                return BadRequest("No such user.");

            Group? group = await _context.Groups
                .Where(g => g.GroupID == request.GroupID)
                .Where(g => g.Participants.Contains(user))
                .Include(g => g.Activities)
                .FirstOrDefaultAsync();
            if (group == null)
                return BadRequest("No such group.");

            Activity activity = new Activity();
            activity.OwnerID = userID;
            activity.Description = "New Activity";
            activity.Group = group;

            user.Activities.Add(activity);
            group.Activities.Add(activity);

            await _context.Activities.AddAsync(activity);
            await _context.SaveChangesAsync();

            return Ok(group.Activities);
        }

        [HttpPost("IsOwner"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult<bool>> IsOwner(ActivityDto request)
        {
            int userID = _userService.GetUserID();
            Activity? activity = await _context.Activities
                .Where(a => a.ActivityID == request.ActivityID)
                .Include(a => a.Group)
                .FirstOrDefaultAsync();
            if (activity == null)
                return BadRequest("No such activity");
            if (activity.Group == null)
                return BadRequest("Unexpected error - Activity not tied to a group");

            return Ok(activity.OwnerID == userID || activity.Group.OwnerID == userID);
        }

        [HttpPost("ReadOne"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult<Activity>> ReadOne(ActivityDto request)
        {
            Activity? activity = await _context.Activities
                .Where(a => a.ActivityID == request.ActivityID)
                .Include(a => a.Group)
                .Include(a => a.Participants)
                .FirstOrDefaultAsync();
            if (activity == null)
                return BadRequest("No such activity");

            return Ok(activity);
        }

        [HttpPost("ReadMany"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult<List<Activity>>> ReadActivities(GroupDto request)
        {
            List<Activity>? activities = await _context.Groups
                .Where(g => g.GroupID == request.GroupID)
                .Select(g => g.Activities)
                .FirstOrDefaultAsync();
            if (activities == null)
                return BadRequest("No such group.");

            return Ok(activities);
        }

        [HttpPut("UpdateActivity"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult> UpdateActivity(Activity request)
        {
            Activity? activity = await _context.Activities
                .Where(a => a.ActivityID == request.ActivityID)
                .Include(a => a.Group).ThenInclude(g => g.Participants)
                .Include(a => a.Participants)
                .FirstOrDefaultAsync();
            if (activity == null)
                return BadRequest("No such activity");
            if (activity.Group == null)
                return BadRequest("Unexpected error - Activity not tied to a Group.");

            // Updates all scalary information.
            var entry = _context.Entry(activity);
            entry.CurrentValues.SetValues(request);

            // Update Participants
            List<User> participants = activity.Group.Participants.ToList();

            foreach (User p in participants)
            {
                User? participant = request.Participants.SingleOrDefault(u => u.UserID == p.UserID);
                if (participant == null && activity.Participants.Contains(p))
                    entry.Entity.Participants.Remove(await _context.Users.FindAsync(p.UserID));
            }

            foreach (User participant in request.Participants)
            {
                User? p = activity.Participants.SingleOrDefault(u => u.UserID == participant.UserID);
                if (p == null)
                {
                    p = await _context.Users
                        .FindAsync(participant.UserID);
                    if (p == null)
                        continue;
                }
                entry.Entity.Participants.Add(await _context.Users.FindAsync(p.UserID));
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("Delete"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult> Delete(ActivityDto request)
        {
            int userID = _userService.GetUserID();
            Activity? activity = await _context.Activities
                .Where(a => a.ActivityID == request.ActivityID)
                .Include(a => a.Group)
                .FirstOrDefaultAsync();
            if (activity == null)
                return BadRequest("No such activity.");
            if (activity.Group == null)
                return BadRequest("Unexpected error - Activity not tied to a Group.");

            if (activity.OwnerID != userID || activity.Group.OwnerID != userID)
                return Unauthorized("Only activity owners or the group owner can delete an activity");

            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
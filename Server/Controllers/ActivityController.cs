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
        public async Task<ActionResult<List<Activity>>> Create(GroupDto requestedGroup)
        {
            int userID = _userService.GetUserID();
            var user = await _context.Users.FindAsync(userID);
            if (user == null)
                return BadRequest("No such user.");

            var group = await _context.Groups
                .Where(g => g.GroupID == requestedGroup.GroupID)
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

        [HttpPost("ReadOne"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult<Activity>> ReadOne(ActivityDto request)
        {
            var activity = await _context.Activities
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
            var activities = await _context.Groups
                .Where(g => g.GroupID == request.GroupID)
                .Select(g => g.Activities)
                .ToListAsync();

            return Ok(activities);
        }

        [HttpPut("UpdateActivity"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult> UpdateActivity(Activity request)
        {
            var activity = await _context.Activities
                .Where(a => a.ActivityID == request.ActivityID)
                .Include(a => a.Group).ThenInclude(a => a.Participants)
                .Include(a => a.Participants)
                .FirstOrDefaultAsync();
            if (activity == null)
                return BadRequest("No such activity");

            // Update Description
            if (activity.Description != request.Description)
                activity.Description = request.Description;

            // Update Participants
            List<User> participants = activity.Group.Participants;
            foreach (User p in participants)
            {
                User? participant = request.Participants.SingleOrDefault(u => u.UserID == p.UserID);
                if (participant == null && activity.Participants.Contains(p))
                    activity.Participants.Remove(p);
                    
            }

            foreach (User participant in request.Participants)
            {
                User? p = activity.Participants.SingleOrDefault(u => u.UserID == participant.UserID);
                if (p == null)
                {
                    p = await _context.Users
                        .FindAsync(participant.UserID);
                }
                    activity.Participants.Add(p);
            }

            // Update amount
            if (activity.Amount != request.Amount)
                activity.Amount = request.Amount;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("Delete"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult> Delete(ActivityDto request)
        {
            int userID = _userService.GetUserID();
            var activity = await _context.Activities
                .Where(a => a.ActivityID == request.ActivityID)
                .Include(a => a.Group)
                .FirstOrDefaultAsync();
            if (activity == null)
                return BadRequest("No such activity.");

            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

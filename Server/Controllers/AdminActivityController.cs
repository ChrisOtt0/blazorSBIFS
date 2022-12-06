using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace blazorSBIFS.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminActivityController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IUserService _userService;

        public AdminActivityController(DataContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        [HttpPost("Create"), Authorize(Roles = "admin")]
        public async Task<ActionResult<List<Activity>>> Create(GroupUserDto request)
        {
            var user = await _context.UserLogins
                .Where(u => u.UserID == request.UserRequest.UserID)
                .Select(u => u.User)
                .FirstOrDefaultAsync();
            if (user == null)
                return BadRequest("No such user.");

            var group = await _context.Groups
                .Where(g => g.GroupID == request.GroupRequest.GroupID)
                .Where(g => g.Participants.Contains(user))
                .Include(g => g.Activities)
                .FirstOrDefaultAsync();
            if (group == null)
                return BadRequest("No such group.");

            Activity activity = new Activity();
            activity.OwnerID = user.UserID;
            activity.Group = group;
            activity.Description = "New Activity";

            user.Activities.Add(activity);
            group.Activities.Add(activity);

            await _context.Activities.AddAsync(activity);
            await _context.SaveChangesAsync();

            return Ok(group.Activities);
        }

        [HttpDelete("Delete"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult> Delete(ActivityDto request)
        {
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

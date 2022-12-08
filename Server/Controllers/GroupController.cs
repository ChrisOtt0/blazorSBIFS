using Azure.Core;
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

        [HttpPut("AddActivity"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult<Group>> AddActivity(GroupActivityDto request)
        {
            if (request.GroupRequest == null || request.ActivityRequest == null)
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
                return Unauthorized("Only the group owner can add activities.");

            Activity? activity = await _context.Activities
                .Where(a => a.ActivityID == request.ActivityRequest.ActivityID)
                .FirstOrDefaultAsync();
            if (activity == null)
                return BadRequest("No such activity.");

            if (group.Activities.Contains(activity))
                return BadRequest("Activity is already added to the group.");

            group.Activities.Add(activity);
            await _context.SaveChangesAsync();

            return Ok(group);
        }
        [HttpDelete("Delete"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult> Delete(GroupDto request)
        {
            int userID = _userService.GetUserID();
            Group? group = await _context.Groups.FirstOrDefaultAsync(g => g.GroupID == request.GroupID && g.OwnerID == userID);
            if (group == null)
                return BadRequest("No such group.");

            if (group.OwnerID != userID)
                return Unauthorized("Only the group owner can delete the group");

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

        [HttpPut("LeaveGroup"), Authorize(Roles = "admin, user")] //Leave group as participant (not owner)
        public async Task<ActionResult> LeaveGroup(GroupDto request)
        {
            int userID = _userService.GetUserID();
            Group? group = await _context.Groups
                .Where(g => g.GroupID == request.GroupID)
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

            // Remove user from activities in the group.
            List<Activity>? activities = await _context.Groups
                .Where(g => g.GroupID == request.GroupID)
                .Select(g => g.Activities)
                .FirstOrDefaultAsync();
            if (activities != null)
            {
                foreach (Activity activity in activities)
                {
                    var activityEntry = _context.Entry(activity);
                    if (activityEntry.Entity.Participants != null && activityEntry.Entity.Participants.Contains(user))
                        activityEntry.Entity.Participants.Remove(user);
                }
            }

            group.Participants.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("Calculate"), Authorize(Roles = "admin, user")]
        public async Task<ActionResult<CalculationDto>> Calculate(GroupDto request)
        {
            Group? group = await _context.Groups
                .Where(g => g.GroupID == request.GroupID)
                .Include(g => g.Participants)
                .Include(g => g.Activities).ThenInclude(a => a.Participants)
                .FirstOrDefaultAsync();
            if (group == null)
                return BadRequest("No such group");
            List<User> participants = group.Participants;
            List<Activity> activities = group.Activities;
            double[,] graph = new double[participants.Count,participants.Count];

            // Creating references for the graph
            Dictionary<User, int> userReference = new Dictionary<User, int>();
            for (int i = 0; i < participants.Count; i++)
            {
                userReference.Add(participants[i], i);
            }
            
            // Filling graph with values
            foreach (Activity activity in activities)
            {
                double amount = Math.Round(((double)activity.Amount / activity.Participants.Count), 2, MidpointRounding.AwayFromZero);
                foreach (User user in activity.Participants)
                {
                    int keyReceiver = userReference.Where(u => u.Key.UserID == activity.OwnerID).FirstOrDefault().Value;
                    int keyPayer = userReference[user];

                    if (keyReceiver == keyPayer) continue;

                    graph[keyReceiver, keyPayer] += amount;
                }
            }

            // Simplify Graph
            GraphTools.SimplifyGraph(ref graph);

            return Ok(new CalculationDto { Results = await ResultsFromGraph(graph, userReference) });
        }

        private async Task<string> ResultsFromGraph(double[,] graph, Dictionary<User, int> reference)
        {
            string results = "";
            Dictionary<int, User> revReference = new Dictionary<int, User>();
            foreach (KeyValuePair<User, int> kvp in reference)
                revReference.Add(kvp.Value, kvp.Key);

            // go through each reference
            // User with name and email owes User with name and email & User with name and email receives from User with name and email:
            // Owes bool to keep track if owes or not, reset upon new reference
            foreach (KeyValuePair<User, int> kvp in reference)
            {
                bool owes = false;
                bool isOwed = false;

                results += "// User: " + kvp.Key.Name
                                       + "\t - " + _context.UserLogins.
                                           Where(u => u.UserID == kvp.Key.UserID)
                                           .FirstAsync().Result.Email
                                       + "//\n";

                // Add what the user owes
                results += "Owes:\n";
                for (int i = 0; i < graph.GetLength(0); i++)
                {
                    if (graph[i, kvp.Value] == 0.0 || i == kvp.Value) continue;
                    User receiver = revReference[i];
                    UserLogin? receiverLogin = await _context.UserLogins.Where(u => u.UserID == receiver.UserID).FirstOrDefaultAsync();
                    if (receiverLogin == null)
                        return "Error in db";

                    results += graph[i, kvp.Value]
                               + ",- to: " + receiver.Name
                               + " - " + receiverLogin.Email
                               + "\n";

                    if (!owes)
                        owes = true;
                }

                if (!owes)
                    results += "Nothing.\n\n";

                // Add what the user is owed
                results += "Is owed:\n";
                for (int i = 0; i < graph.GetLength(1); i++)
                {
                    if (graph[kvp.Value, i] == 0.0 || i == kvp.Value) continue;
                    User receiver = revReference[i];
                    UserLogin? receiverLogin = await _context.UserLogins.Where(u => u.UserID == receiver.UserID).FirstOrDefaultAsync();
                    if (receiverLogin == null)
                        return "Error in db";

                    results += graph[kvp.Value, i]
                               + ",- from: " + receiver.Name
                               + " - " + receiverLogin.Email
                               + "\n";

                    if (!isOwed)
                        isOwed = true;
                }

                if (!isOwed)
                    results += "Nothing.\n\n\n";
                else { results += "\n\n\n"; }
            }

            return results;
        }
    }
}
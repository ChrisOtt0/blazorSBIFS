namespace blazorSBIFS.Server.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetUserID()
        {
            int result = -1;
            if (_httpContextAccessor.HttpContext != null)
            {
                int.TryParse(_httpContextAccessor.HttpContext.User.Identity?.Name, out result);
            }

            return result;
        }
    }
}

namespace blazorSBIFS.Client.Services
{
    public interface IJwtService
    {
        public string JwtToken { get; set; }
    }

    public class JwtService : IJwtService
    {
        public string JwtToken { get; set; } = string.Empty;
    }
}

namespace blazorSBIFS.Client.Services
{
    public interface ITokenService
    {
        public string Jwt { get; set; }
    }

    public class TokenService : ITokenService
    {
        public string Jwt { get; set; } = string.Empty;
    }
}

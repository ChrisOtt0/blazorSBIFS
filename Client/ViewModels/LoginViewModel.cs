using blazorSBIFS.Client.Services;

namespace blazorSBIFS.Client.ViewModels
{
	public interface ILoginViewModel
	{
		public string Email { get; set; }
		public string Password { get; set; }
		public string Message { get; set; }

		public void Login();
	}

	public class LoginViewModel : ILoginViewModel
	{
		private readonly IHttpService _httpService;

		public string Email { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public string Message { get; set; } = string.Empty;

        public LoginViewModel(IHttpService httpService)
		{
			_httpService = httpService;
		}

		public void Login()
		{
			Message = Email + ": " + Password;
		}
	}
}

using blazorSBIFS.Client.Services;
using blazorSBIFS.Shared.DataTransferObjects;

namespace blazorSBIFS.Client.ViewModels
{
	public interface IRegisterViewModel
	{
		public string Name { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public string ConfirmPassword { get; set; }

		public string Message { get; set; }

		public void Submit();
	}

	public class RegisterViewModel : IRegisterViewModel
	{
		private readonly IHttpService _httpService;
		private readonly ITokenService _tokenService;
		string baseUrl = "UserLogin/";

		public string Name { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public string ConfirmPassword { get; set; } = string.Empty;

		public string Message { get; set; } = string.Empty;

		public RegisterViewModel(IHttpService httpService, ITokenService tokenService)
		{
			_httpService = httpService;
			_tokenService = tokenService;
		}

        public async void Submit()
		{
			Message = "Submitting userdata..";
			string url = "Register";

			// Inputvalidation
			if (Name == string.Empty || Name == "")
			{
				Message = "Please enter a name to register your account.";
				return;
			}

			if (Email == string.Empty || Email == "")
			{
				Message = "Please enter an email to register your account.";
				return;
			}

			if (Password != ConfirmPassword)
			{
				Message = "Passwords do not match.";
			}

			IJson data = new UserRegisterDto
			{
				Name = this.Name,
				Email = this.Email,
				Password = this.Password
			};
			HttpResponseMessage response = await _httpService.Post(baseUrl + url, data);

			switch( (int) response.StatusCode )
			{
				case 204:
					Message = "Account created succesfully. Login to access the service.";
					break;

				case 422:
					Message = await response.Content.ReadAsStringAsync();
					break;

				case 500:
					Message = "Server Error. Check logs at: {log_path}";
                    break;

				default:
                    Message = ((int)response.StatusCode).ToString() + ": " + await response.Content.ReadAsStringAsync();
                    break;
            }
		}
    }
}

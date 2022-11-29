using blazorSBIFS.Client.Services;
using blazorSBIFS.Shared.DataTransferObjects;
using System.Diagnostics;
using System.Net.Http.Json;

namespace blazorSBIFS.Client.ViewModels
{
	public interface IRegisterViewModel
	{
		public string Name { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public string ConfirmPassword { get; set; }

		public string Message { get; set; }

		public Task Submit();
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

        public async Task Submit()
		{
			Message = "Loading...";
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
				case 200:
					TokenDto json = await response.Content.ReadFromJsonAsync<TokenDto>();
                    _tokenService.Jwt = json.Jwt;

					// Navigate to profile page
					Message = "Success!";
					break;

				case 422:
					Message = "Email or password is already taken.";
					Console.WriteLine(Message);
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

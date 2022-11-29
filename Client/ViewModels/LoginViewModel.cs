using blazorSBIFS.Client.Services;
using blazorSBIFS.Shared.DataTransferObjects;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace blazorSBIFS.Client.ViewModels
{
	public interface ILoginViewModel
	{
		public string Email { get; set; }
		public string Password { get; set; }
		public string Message { get; set; }

		public Task Login();
	}

	public class LoginViewModel : ILoginViewModel
	{
		private readonly IHttpService _httpService;
		private readonly ITokenService _tokenService;
		private readonly NavigationManager _navigationManager;
		string baseUrl = "UserLogin/";

		public string Email { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public string Message { get; set; } = string.Empty;

        public LoginViewModel(
			IHttpService httpService, 
			ITokenService tokenService,
			NavigationManager navigationManager)
		{
			_httpService = httpService;
			_tokenService = tokenService;
			_navigationManager = navigationManager;
		}

		public async Task Login()
		{
			Message = "Connecting...";
			string url = "Login";

			if (Email == string.Empty || Email == "")
			{
				Message = "Please enter an email to login.";
				return;
			}

			if (Password == string.Empty || Password == "")
			{
				Message = "No password given.";
				return;
			}

			IJson data = new UserLoginDto
			{
				Email = this.Email,
				Password = this.Password,
			};
			HttpResponseMessage response = await _httpService.Post(baseUrl + url, data);
            
			switch ((int)response.StatusCode)
            {
                // OK
                case 200:
                    TokenDto json = await response.Content.ReadFromJsonAsync<TokenDto>();
					_tokenService.Jwt = json.Jwt;

                    _navigationManager.NavigateTo("/");
                    break;

                // BadRequest
                case 400:
                // Unauthorized
                case 401:
                    Message = await response.Content.ReadAsStringAsync();
                    break;

                // Server Error
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

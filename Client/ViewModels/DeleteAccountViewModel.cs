using blazorSBIFS.Client.Services;
using blazorSBIFS.Shared.DataTransferObjects;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace blazorSBIFS.Client.ViewModels
{
	public interface IDeleteAccountViewModel
	{
		public string Email { get; set; }
		public string Password { get; set; }
		public string Message { get; set; }

		public Action StateHasChangedDelegate { get; set; }

		public void Submit();
	}

	public class DeleteAccountViewModel : IDeleteAccountViewModel
	{
		private readonly IHttpService _httpService;
		private readonly ITokenService _tokenService;
		private readonly NavigationManager _navigationManager;

		string baseUrl = "User/";

		public string Email { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public string Message { get; set; } = string.Empty;

        public Action StateHasChangedDelegate { get; set; }

        public DeleteAccountViewModel(IHttpService httpService, ITokenService tokenService, NavigationManager navigationManager)
		{
			_httpService = httpService;
			_tokenService = tokenService;
			_navigationManager = navigationManager;
		}

		public async void Submit()
		{
			if (Email == string.Empty || Email == "")
			{
				Message = "If you want to delete your account, please enter your email.";
				StateHasChangedDelegate();
				return;
			}

			if (Password == String.Empty || Password == "")
			{
                Message = "If you want to delete your account, please enter your password.";
                StateHasChangedDelegate();
                return;
            }

			Message = "Deleting user. Thank you for using the service!";
			StateHasChangedDelegate();

			string url = "Delete";
			IJson data = new UserLoginDto
			{
				Email = this.Email,
				Password = this.Password
			};
			HttpResponseMessage response = await _httpService.Delete(baseUrl + url, data);
			
			if (!response.IsSuccessStatusCode)
			{
				Message = await response.Content.ReadAsStringAsync();
				StateHasChangedDelegate();
				return;
			}

			_tokenService.Jwt = string.Empty;
			_navigationManager.NavigateTo("/");
		}
	}
}

using blazorSBIFS.Shared.DataTransferObjects;
using Microsoft.AspNetCore.Components;

namespace blazorSBIFS.Client.Pages
{
	public partial class DeleteAccountPage
	{
        string baseUrl = "User/";

        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        protected override async void OnInitialized()
        {
            if (_token.Jwt == string.Empty) _nav.NavigateTo("login");
        }

        public async void Submit()
        {
            if (Email == string.Empty || Email == "")
            {
                Message = "If you want to delete your account, please enter your email.";
                StateHasChanged();
                return;
            }

            if (Password == String.Empty || Password == "")
            {
                Message = "If you want to delete your account, please enter your password.";
                StateHasChanged();
                return;
            }

            Message = "Deleting user..";
            StateHasChanged();

            string url = "Delete";
            IJson data = new UserLoginDto
            {
                Email = this.Email,
                Password = this.Password
            };
            HttpResponseMessage response = await _http.Delete(baseUrl + url, data);

            if (!response.IsSuccessStatusCode)
            {
                Message = await response.Content.ReadAsStringAsync();
                StateHasChanged();
                return;
            }

            _token.Jwt = string.Empty;
            _nav.NavigateTo("/");
        }
    }
}

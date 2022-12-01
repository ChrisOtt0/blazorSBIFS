using blazorSBIFS.Client.Services;
using blazorSBIFS.Shared.DataTransferObjects;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace blazorSBIFS.Client.Pages
{
	public partial class LoginPage
	{
        string baseUrl = "UserLogin/";

        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public async void Login()
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
            HttpResponseMessage response = await _http.Post(baseUrl + url, data);

            switch ((int)response.StatusCode)
            {
                // OK
                case 200:
                    TokenDto json = await response.Content.ReadFromJsonAsync<TokenDto>();
                    _token.Jwt = json.Jwt;
                    _http.AddAuthorization(_token.Jwt);

                    _nav.NavigateTo("/");
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

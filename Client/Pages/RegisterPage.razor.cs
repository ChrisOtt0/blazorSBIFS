using blazorSBIFS.Shared.DataTransferObjects;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace blazorSBIFS.Client.Pages
{
	public partial class RegisterPage
	{
        string baseUrl = "UserLogin/";

        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public async Task Submit()
        {
            Message = "Loading...";
            string url = "Register";

            // Inputvalidation
            if (Name == string.Empty || Name == "")
            {
                Message = "Please enter a name to register your account.";
                StateHasChanged();
                return;
            }

            if (Email == string.Empty || Email == "")
            {
                Message = "Please enter an email to register your account.";
                StateHasChanged();
                return;
            }

            if (Password != ConfirmPassword)
            {
                Message = "Passwords do not match.";
                StateHasChanged();
                return;
            }

            IJson data = new UserRegisterDto
            {
                Name = this.Name,
                Email = this.Email,
                Password = this.Password
            };
            HttpResponseMessage response = await _http.Post(baseUrl + url, data);

            switch ((int)response.StatusCode)
            {
                case 200:
                    TokenDto json = await response.Content.ReadFromJsonAsync<TokenDto>();
                    _token.Jwt = json.Jwt;
                    _http.AddAuthorization(_token.Jwt);

                    _nav.NavigateTo("/");
                    break;

                case 422:
                    Message = await response.Content.ReadAsStringAsync();
                    StateHasChanged();
                    Console.WriteLine(Message);
                    break;

                case 500:
                    Message = "Server Error. Check logs at: {log_path}";
                    StateHasChanged();
                    break;

                default:
                    Message = ((int)response.StatusCode).ToString() + ": " + await response.Content.ReadAsStringAsync();
                    StateHasChanged();
                    break;
            }
        }
    }
}

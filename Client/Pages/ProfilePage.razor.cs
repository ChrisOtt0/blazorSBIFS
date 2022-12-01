using blazorSBIFS.Shared.DataTransferObjects;
using System.Net.Http.Json;

namespace blazorSBIFS.Client.Pages
{
	public partial class ProfilePage
	{
        string baseUrl = "User/";

        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNew { get; set; } = string.Empty;

        public string ChangesMessage { get; set; } = string.Empty;
        public string PasswordMessage { get; set; } = string.Empty;

        protected override async void OnInitialized()
        {
            if (_token.Jwt == string.Empty) _nav.NavigateTo("login");

            string url = "Read";
            HttpResponseMessage response = await _http.Get(baseUrl + url);

            // Handle errors
            if (!response.IsSuccessStatusCode) return;

            UserDto? user = await response.Content.ReadFromJsonAsync<UserDto>();
            if (user == null) return;

            Name = user.Name;
            Email = user.Email;
            StateHasChanged();
        }
        public async void SaveChanges()
        {
            string url = "UpdateUser";
            ChangesMessage = "Saving changes...";
            StateHasChanged();

            IJson data = new UserDto
            {
                Name = this.Name,
                Email = this.Email
            };
            HttpResponseMessage response = await _http.Put(baseUrl + url, data);
            if (!response.IsSuccessStatusCode)
            {
                ChangesMessage = await response.Content.ReadAsStringAsync();
                StateHasChanged();
                return;
            }

            ChangesMessage = "Update successful!";
            StateHasChanged();
        }

        public async void UpdatePassword()
        {
            string url = "UpdatePassword";

            if (NewPassword != ConfirmNew)
            {
                PasswordMessage = "Passwords do not match.";
                StateHasChanged();
                return;
            }

            PasswordMessage = "Updating password...";
            StateHasChanged();

            IJson data = new PasswordDto
            {
                OldPassword = this.OldPassword,
                NewPassword = this.NewPassword,
            };
            HttpResponseMessage response = await _http.Put(baseUrl + url, data);
            if (!response.IsSuccessStatusCode)
            {
                PasswordMessage = await response.Content.ReadAsStringAsync();
                StateHasChanged();
                return;
            }

            PasswordMessage = "Password updated successfully!";
            StateHasChanged();
        }
    }
}

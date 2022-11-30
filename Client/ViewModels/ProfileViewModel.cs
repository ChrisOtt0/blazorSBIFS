using blazorSBIFS.Client.Pages;
using blazorSBIFS.Client.Services;
using blazorSBIFS.Shared.DataTransferObjects;
using blazorSBIFS.Shared.Models;

namespace blazorSBIFS.Client.ViewModels
{
    public interface IProfileViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNew { get; set; }

        public string ChangesMessage { get; set; }
        public string PasswordMessage { get; set; }

        public string BaseUrl { get; }

        public Action StateHasChangedDelegate { get; set; }

        public void SaveChanges();
        public void UpdatePassword();
    }

    public class ProfileViewModel : IProfileViewModel
    {
        private readonly IHttpService _httpService;

        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNew { get; set; } = string.Empty; 

        public string ChangesMessage { get; set; } = string.Empty;
        public string PasswordMessage { get; set; } = string.Empty;

        public string BaseUrl { get; } = "User/";

        public Action StateHasChangedDelegate { get; set; }

        public ProfileViewModel(IHttpService httpService)
        {
            _httpService = httpService;
        }

        public async void SaveChanges()
        {
            string url = "UpdateUser";
            ChangesMessage = "Saving changes...";
            StateHasChangedDelegate();

            IJson data = new UserDto
            {
                Name = this.Name,
                Email = this.Email
            };
            HttpResponseMessage response = await _httpService.Put(BaseUrl + url, data);
            if (!response.IsSuccessStatusCode)
            {
                ChangesMessage = await response.Content.ReadAsStringAsync();
                StateHasChangedDelegate();
                return;
            }

            ChangesMessage = "Update successful!";
            StateHasChangedDelegate();
        }

        public async void UpdatePassword()
        {
            string url = "UpdatePassword";

            if (NewPassword != ConfirmNew)
            {
                PasswordMessage = "Passwords do not match.";
                StateHasChangedDelegate();
                return;
            }

            PasswordMessage = "Updating password...";
            StateHasChangedDelegate();

            IJson data = new PasswordDto
            {
                OldPassword = this.OldPassword,
                NewPassword = this.NewPassword,
            };
            HttpResponseMessage response = await _httpService.Put(BaseUrl + url, data);
            if (!response.IsSuccessStatusCode)
            {
                PasswordMessage = await response.Content.ReadAsStringAsync();
                StateHasChangedDelegate();
                return;
            }

            PasswordMessage = "Password updated successfully!";
            StateHasChangedDelegate();
        }
    }
}

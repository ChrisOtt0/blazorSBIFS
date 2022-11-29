using blazorSBIFS.Client.Services;
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

        public ProfileViewModel(IHttpService httpService)
        {
            _httpService = httpService;

            // Add logic to get user from database, and fill in Name and Email properties on this ViewModel
            Name = "John";
            Email = "test@gmail.com";
        }

        public void SaveChanges()
        {
            ChangesMessage = "Saving changes...";
        }

        public void UpdatePassword()
        {
            PasswordMessage = "Updating password...";
        }
    }
}

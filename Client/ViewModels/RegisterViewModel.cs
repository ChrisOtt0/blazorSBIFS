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
        public string Name { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public string ConfirmPassword { get; set; } = string.Empty;

		public string Message { get; set; } = string.Empty;

        public void Submit()
		{
			Message = "Submitting userdata..";
		}
    }
}

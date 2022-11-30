using blazorSBIFS.Client.Pages;
using blazorSBIFS.Client.Services;
using blazorSBIFS.Shared.DataTransferObjects;
using blazorSBIFS.Shared.Models;

namespace blazorSBIFS.Client.ViewModels
{
	public interface IGroupViewModel
	{
		public string GroupName { get; set; }
		public string BaseUrl { get; }
		public Group Group { get; set; }
		public int GroupID { get; set; }

		public Action StateHasChangedDelegate { get; set; }

		public void UpdateName();
        public void RemoveParticipant(User user); 
    }

	public class GroupViewModel : IGroupViewModel
	{
		private readonly IHttpService _httpService;

        public string GroupName { get; set; }
        public string BaseUrl { get; } = "Group/";
		public Group Group { get; set; }
        public int GroupID { get; set; }

        public Action StateHasChangedDelegate { get; set; }

        public GroupViewModel(IHttpService httpService)
		{
			_httpService = httpService;
		}

        public void UpdateName()
		{
			GroupName = Group.Name;
			StateHasChangedDelegate();
		}

        public void RemoveParticipant(User user)
		{
			Group.Participants.Remove(user);
			StateHasChangedDelegate();
		}
    }
}

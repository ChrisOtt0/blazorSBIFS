using blazorSBIFS.Client.Pages;
using blazorSBIFS.Client.Services;
using blazorSBIFS.Shared.Models;

namespace blazorSBIFS.Client.ViewModels
{
	public interface IGroupViewModel
	{
		public string BaseUrl { get; }
		public Group Group { get; set; }
		public int GroupID { get; set; }
	}

	public class GroupViewModel : IGroupViewModel
	{
		private readonly IHttpService _httpService;

        public string BaseUrl { get; } = "Group/";
		public Group Group { get; set; }
        public int GroupID { get; set; }

		public GroupViewModel(IHttpService httpService)
		{
			_httpService = httpService;
		}


    }
}

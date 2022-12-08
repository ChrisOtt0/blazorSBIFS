using blazorSBIFS.Shared.DataTransferObjects;
using blazorSBIFS.Shared.Models;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace blazorSBIFS.Client.Pages
{
	public partial class ActivityPage
	{
		string baseUrl = "Activity/";

		[Parameter]
		public int ActivityID { get; set; }
		public Activity? Activity { get; set; }
		public List<User>? GroupParticipants { get; set; } = new List<User>();
		public List<User>? Responsible { get; set; } = new List<User>();
		public double? Amount { get; set; } = 0.0;
		public string FeedbackLabel { get; set; } = string.Empty;

		protected override async void OnInitialized()
		{
			base.OnInitialized();
			if (_token.Jwt == string.Empty) _nav.NavigateTo("login");

			// Read Activity
			string url = "ReadOne";
			IJson data = new ActivityDto { ActivityID= ActivityID };
			HttpResponseMessage response = await _http.Post(baseUrl+ url, data);
			if (!response.IsSuccessStatusCode)
			{
				FeedbackLabel = ((int)response.StatusCode).ToString() + ": " + await response.Content.ReadAsStringAsync();
				this.ActivityID = 0;
				this.Activity = new Activity
				{
					ActivityID = this.ActivityID,
					Description = "Not found.",
					Amount = 0.0,
				};
				return;
			}

			Activity = await response.Content.ReadFromJsonAsync<Activity>();
			if (Activity == null)
			{
				FeedbackLabel = "Error in the API. Please contact a developer.";
				this.ActivityID = 0;
				this.Activity = new Activity
				{
					ActivityID = this.ActivityID,
					Description = "Not found.",
					Amount = 0.0,
				};
				return;
			}

			Responsible = Activity.Participants;
			Amount = Activity.Amount;

			// Read GroupParticipants
			url = "Group/ReadParticipants";
			if (Activity.Group == null)
			{
				FeedbackLabel = "Activity has no Group attached";
				return;
			}
			data = new GroupDto { GroupID = Activity.Group.GroupID };

			response = await _http.Post(url, data);
			if (!response.IsSuccessStatusCode)
			{
				FeedbackLabel = ((int)response.StatusCode).ToString() + ": " + await response.Content.ReadAsStringAsync();
				return;
			}

			GroupParticipants = await response.Content.ReadFromJsonAsync<List<User>>();
			if (GroupParticipants == null)
			{
				FeedbackLabel = "Connected Group has no participants.";
				return;
			}

			StateHasChanged();
		}
	}
}

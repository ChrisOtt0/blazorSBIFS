using blazorSBIFS.Shared.DataTransferObjects;
using blazorSBIFS.Shared.HelperModels;
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
		public string ActivityDescription { get; set; } = "Loading..";
		public string ShortenedActivityDescription { get; set; } = "Loading..";
		public Activity? Activity { get; set; }
		public List<User>? GroupParticipants { get; set; } = new List<User>();
		public List<ResponsibleParticipant> ResponsibleParticipants { get; set; } = new List<ResponsibleParticipant>();
		public double Amount { get; set; }
		public string FeedbackLabel { get; set; } = string.Empty;
		public bool IsOwner { get; set; } = false;

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
				StateHasChanged();
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
				StateHasChanged();
				return;
			}
			Amount = Activity.Amount == null ? 0.0 : Activity.Amount.Value;

			// Check if IsOwner
			url = "IsOwner";
			response = await _http.Post(baseUrl + url, data);
			if (!response.IsSuccessStatusCode)
			{
				FeedbackLabel = ((int)response.StatusCode).ToString()
					+ ": " + await response.Content.ReadAsStringAsync();
				StateHasChanged();
				return;
			}
			IsOwner = await response.Content.ReadFromJsonAsync<bool>();

			// Read GroupParticipants
			url = "Group/ReadParticipants";
			if (Activity.Group == null)
			{
				FeedbackLabel = "Activity has no Group attached";
				StateHasChanged();
				return;
			}
			data = new GroupDto { GroupID = Activity.Group.GroupID };

			response = await _http.Post(url, data);
			if (!response.IsSuccessStatusCode)
			{
				FeedbackLabel = ((int)response.StatusCode).ToString() + ": " + await response.Content.ReadAsStringAsync();
				StateHasChanged();
				return;
			}

			GroupParticipants = await response.Content.ReadFromJsonAsync<List<User>>();
			if (GroupParticipants == null)
			{
				FeedbackLabel = "Connected Group has no participants.";
				StateHasChanged();
			}

			if (Activity.Participants != null && Activity.Participants.Count > 0)
			{
				foreach (User participant in GroupParticipants)
				{
					if (Activity.Participants.Where(p => p.UserID == participant.UserID).FirstOrDefault() != null)
						ResponsibleParticipants.Add(new ResponsibleParticipant { User = participant, Name = participant.Name, IsResponsible = true });
					else
						ResponsibleParticipants.Add(new ResponsibleParticipant { User = participant, Name = participant.Name, IsResponsible = false });
                }
			}
			else
			{
				Console.WriteLine("Participants null or Empty");
				foreach (User participant in GroupParticipants)
					ResponsibleParticipants.Add(new ResponsibleParticipant { User = participant, Name = participant.Name, IsResponsible = true });
			}


			if (Activity.Description == null)
			{
				ShortenedActivityDescription = "New Activity";
				StateHasChanged();
				return;
			}
			if (Activity.Description.IndexOf("\n") > -1)
				ShortenedActivityDescription = Activity.Description.Substring(0, Activity.Description.IndexOf("\n"));
			else if (Activity.Description.IndexOf(" ") > -1)
				ShortenedActivityDescription = Activity.Description.Substring(0, Activity.Description.IndexOf(" "));
			else
				ShortenedActivityDescription = Activity.Description;
			ActivityDescription = Activity.Description;

			StateHasChanged();
		}

		public async void Save()
		{
			if (ActivityID == 0 || Activity == null)
			{
				FeedbackLabel = "No Activity to save.";
				StateHasChanged();
				return;
			}

			string url = "UpdateActivity";

			// Update name
			Activity.Description = ActivityDescription;

			// Update amount
			Activity.Amount = Amount;

			// Update Participants
			if (Activity.Participants == null) Activity.Participants = new List<User>();
			else Activity.Participants.Clear();
			foreach (ResponsibleParticipant responsible in ResponsibleParticipants)
			{
				if (responsible.IsResponsible)
					Activity.Participants.Add(responsible.User);
			}

			// Store persistently
			HttpResponseMessage response = await _http.Put(baseUrl + url, Activity);
			if (!response.IsSuccessStatusCode)
			{
				FeedbackLabel = ((int)response.StatusCode).ToString()
					+ ": " + await response.Content.ReadAsStringAsync();
				StateHasChanged();
				return;
			}

			ClearMessages();
			if (Activity.Group != null)
				_nav.NavigateTo($"/group/{Activity.Group.GroupID}");
			else
				_nav.NavigateTo("/groups");
		}

		public void Cancel()
		{
			ClearMessages();

			if (Activity.Group != null)
				_nav.NavigateTo($"/group/{Activity.Group.GroupID}");
			else
				_nav.NavigateTo("/groups");
		}

		public async void Delete()
		{
			if (ActivityID == 0)
			{
				FeedbackLabel = "No Activity to delete.";
				StateHasChanged();
				return;
			}

			string url = "Delete";
			IJson data = new ActivityDto { ActivityID = this.ActivityID };

			HttpResponseMessage response = await _http.Delete(baseUrl + url, data);
			if (!response.IsSuccessStatusCode)
			{
				FeedbackLabel = ((int)response.StatusCode).ToString()
					+ ": " + await response.Content.ReadAsStringAsync();
				StateHasChanged();
				return;
			}

			ClearMessages();
			if (Activity.Group != null)
				_nav.NavigateTo($"/group/{Activity.Group.GroupID}");
			else
				_nav.NavigateTo("/groups");
		}

		private void ClearMessages()
		{
			FeedbackLabel = string.Empty;
			StateHasChanged();
		}
	}
}

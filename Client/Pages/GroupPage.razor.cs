using blazorSBIFS.Shared.DataTransferObjects;
using blazorSBIFS.Shared.Models;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json.Serialization;

namespace blazorSBIFS.Client.Pages
{
    public partial class GroupPage
    {
        string baseUrl = "Group/";

        [Parameter]
        public int GroupID { get; set; }
        public string GroupName { get; set; } = "Loading..";
        public Group? Group { get; set; }
        public string SearchEmail { get; set; } = string.Empty;
        public bool IsOwner { get; set; } = false;
        public string ActivityMessage { get; set; } = string.Empty;
        public string ParticipantMessage { get; set; } = string.Empty;

        protected override async void OnInitialized()
        {
            base.OnInitialized();
            if (_token.Jwt == string.Empty) _nav.NavigateTo("login");

            string url = "ReadOne";
            IJson data = new GroupDto
            {
                GroupID = this.GroupID
            };
            HttpResponseMessage response = await _http.Post(baseUrl + url, data);

            // catching errors
            if (!response.IsSuccessStatusCode)
            {
                GroupID = 0;
                StateHasChanged();
                return;
            }

            Group? g = await response.Content.ReadFromJsonAsync<Group>();
            if (g == null)
            {
                GroupID = 0;
                StateHasChanged();
                return;
            }

            Group = g;
            GroupName = g.Name;

            // Check if Owner
            url = "IsOwner";
            data = new GroupDto { GroupID = this.GroupID };

            response = await _http.Post(baseUrl + url, data);
            if (!response.IsSuccessStatusCode)
            {
                ActivityMessage = "Error defining owner.";
                GroupID = 0;
                StateHasChanged();
                return;
            }

            IsOwner = await response.Content.ReadFromJsonAsync<bool>();
            StateHasChanged();
        }

        public async void UpdateName()
        {
            if (GroupName == string.Empty)
            {
                ActivityMessage = "Group name cannot be empty.";
                StateHasChanged();
                return;
            }

            string url = "UpdateName";
            IJson data = new GroupNameDto
            {
                GroupID = this.GroupID,
                Name = GroupName
            };

            HttpResponseMessage response = await _http.Put(baseUrl + url, data);
            if (!response.IsSuccessStatusCode)
            {
                ActivityMessage = ((int)response.StatusCode).ToString()
                    + ": " + await response.Content.ReadAsStringAsync();
                StateHasChanged();
                return;
            }

            Group? group = await response.Content.ReadFromJsonAsync<Group>();
            if (group == null)
            {
                ActivityMessage = "Something went wrong.";
                StateHasChanged();
                return;
            }

            ActivityMessage = "Name updated successfully.";
            ParticipantMessage = string.Empty;
            Group = group;
            StateHasChanged();
        }

        public async void AddParticipant()
        {
            if (GroupID == 0)
            {
                ParticipantMessage = "No group to add participants to.";
                StateHasChanged();
                return;
            }

            string url = "AddParticipant";
            GroupDto gDto = new GroupDto { GroupID = this.GroupID };
            EmailDto eDto = new EmailDto { Email = SearchEmail };
            IJson data = new GroupEmailDto
            {
                GroupRequest = gDto,
                EmailRequest = eDto,
            };

            HttpResponseMessage response = await _http.Put(baseUrl + url, data);
            if (!response.IsSuccessStatusCode)
            {
                ParticipantMessage = ((int)response.StatusCode).ToString()
                    + ": " + await response.Content.ReadAsStringAsync();
                StateHasChanged();
                return;
            }

            Group? group = await response.Content.ReadFromJsonAsync<Group>();
            if (group == null)
            {
                ParticipantMessage = "Unexpected error.";
                GroupID = 0;
                StateHasChanged();
                return;
            }

            Group = group;
            ClearMessages();
        }

        public async void RemoveParticipant(User user)
        {
			if (GroupID == 0)
			{
				ParticipantMessage = "No group to add participants to.";
				StateHasChanged();
				return;
			}

            string url = "RemoveParticipant";
            GroupDto gDto = new GroupDto { GroupID = this.GroupID };
            UserDto uDto = new UserDto { UserID = user.UserID };
            IJson data = new GroupUserDto
            {
                GroupRequest = gDto,
                UserRequest = uDto,
            };

            HttpResponseMessage response = await _http.Put(baseUrl + url, data);
            if (!response.IsSuccessStatusCode)
            {
                ParticipantMessage = ((int)response.StatusCode).ToString()
                    + ": " + await response.Content.ReadAsStringAsync();
                StateHasChanged();
                return;
            }

            Group? group = await response.Content.ReadFromJsonAsync<Group>();
            if (group == null)
            {
                ParticipantMessage = "Unexpected error.";
                StateHasChanged();
                return;
            }

            Group = group;
            ClearMessages();
		}

        public async void AddActivity()
        {
            if (GroupID == 0)
            {
                ActivityMessage = "No group to add activity to.";
                StateHasChanged();
                return;
            }

            string url = "Activity/Create";
            IJson data = new GroupDto { GroupID = this.GroupID };

            HttpResponseMessage response = await _http.Post(url, data);
            if (!response.IsSuccessStatusCode)
            {
                ActivityMessage = ((int)response.StatusCode).ToString()
                    + ": " + await response.Content.ReadAsStringAsync();
                StateHasChanged();
                return;
            }

            List<Activity>? activities = await response.Content.ReadFromJsonAsync<List<Activity>>();
            if (activities == null)
            {
                ActivityMessage = "Unexpected error.";
                StateHasChanged();
                return;
            }

            if (Group == null)
            {
                ActivityMessage = "Unexpected error.";
                StateHasChanged();
                return;
            }

            Group.Activities = activities;
            ClearMessages();
        }

        public void EditActivity(Activity activity)
        {
            ClearMessages();
            _nav.NavigateTo($"/activity/{activity.ActivityID}");
        }

        public void GoBack()
        {
            _nav.NavigateTo("/groups");
        }

        public void Calculate()
        {
            if (GroupID == 0)
            {
                ActivityMessage = "Cannot calculate bill splitting with no group.";
                StateHasChanged();
                return;
            }

            _nav.NavigateTo($"/output/{GroupID}");
        }

        public async void LeaveGroup()
        {
            if (GroupID == 0)
            {
                ActivityMessage = "No group to leave.";
                StateHasChanged();
                return;
            }

            string url = "LeaveGroup";
            IJson data = new GroupDto() { GroupID = this.GroupID };

            HttpResponseMessage response = await _http.Put(baseUrl+ url, data);
            if (!response.IsSuccessStatusCode)
            {
                ActivityMessage = ((int)response.StatusCode).ToString()
                    + ": " + await response.Content.ReadAsStringAsync();
                StateHasChanged();
                return;
            }

            ClearMessages();
            _nav.NavigateTo("/groups");
        }

        public async void DeleteGroup()
        {
            if (GroupID == 0)
            {
                ActivityMessage = "No group to delete.";
                StateHasChanged();
                return;
            }

            string url = "Delete";
            IJson data = new GroupDto { GroupID = this.GroupID };

            HttpResponseMessage response = await _http.Delete(baseUrl + url, data);
            if (!response.IsSuccessStatusCode)
            {
                ActivityMessage = ((int)response.StatusCode).ToString()
                    + ": " + await response.Content.ReadAsStringAsync();
                StateHasChanged();
                return;
            }

            ClearMessages();
            _nav.NavigateTo("/groups");
        }

        public string ShortenedActivityDescription(Activity activity)
        {
            if (activity.Description == null || activity.Description == string.Empty) return "New Activity";

            if (activity.Description.IndexOf("\n") > -1)
                return activity.Description.Substring(0, activity.Description.IndexOf("\n"));
            else if (activity.Description.IndexOf(" ") > -1)
                return activity.Description.Substring(0, activity.Description.IndexOf(" "));
            else
                return activity.Description;
        }

        private void ClearMessages()
        {
            ActivityMessage = string.Empty;
            ParticipantMessage = string.Empty;
            StateHasChanged();
        }
        //public void RemoveUsersActivity(Activity activity)
        //{
        //    string url = "Delete";
        //    IJson data = new ActivityDto()
        //    {
        //        ActivityID = activity.ActivityID
        //    };
        //    HttpResponseMessage response = _http.Post(url, data).Result;
        //    if (!response.IsSuccessStatusCode)
        //    {
        //        GroupID = 0;
        //        return;
        //    }
        //    ReadGroupData();
        //}
        //public void AddUsersActivity(Activity activity)
        //{
        //    string url = "Create";
        //    IJson data = new GroupDto()
        //    {
        //        GroupID = GroupID
        //    };
        //    HttpResponseMessage response = _http.Post(url, data).Result;
        //    if (!response.IsSuccessStatusCode)
        //    {
        //        GroupID = 0;
        //        return;
        //    }
        //    ReadGroupData();
        //}
    }
}
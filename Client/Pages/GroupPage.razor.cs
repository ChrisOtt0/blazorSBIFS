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
        public string GroupName { get; set; } = "not found.";
        public Group? Group { get; set; }
        
        protected override async void OnInitialized()
        {
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
                return;
            }

            Group? g = await response.Content.ReadFromJsonAsync<Group>();
            if (g == null)
            {
                GroupID = 0;
                return;
            }

            Group = g;
            GroupName = g.Name;
            StateHasChanged();
        }

        public void UpdateName()
        {
            GroupName = Group.Name;
            StateHasChanged();
        }

        public void RemoveParticipant(User user)
        {
            Group.Participants.Remove(user);
            StateHasChanged();
        }
        public void AddParticipant(User user)
        {
            Group.Participants.Add(user);
            StateHasChanged();
        }
        public void UpdateGroup()
        {
            string url = "Update";
            IJson data = new Group
            {
                GroupID = GroupID,
                Name = Group.Name,
                Participants = Group.Participants
            };
            HttpResponseMessage response = _http.Put(baseUrl + url, data).Result;
            if (!response.IsSuccessStatusCode)
            {
                GroupID = 0;
                return;
            }
            UpdateName();
        }
        public void ReadGroupData()
        {
            string url = "ReadOne";
            IJson data = new GroupDto()
            {
                GroupID = GroupID
            };
            HttpResponseMessage response = _http.Post(baseUrl + url, data).Result;
            if (!response.IsSuccessStatusCode)
            {
                GroupID = 0;
                return;
            }
            Group? g = response.Content.ReadFromJsonAsync<Group>().Result;
            if (g == null)
            {
                GroupID = 0;
                return;
            }
            Group = g;
            UpdateName();
        }
        public void OwnerDeletesGroup()
        {
            string url = "Delete";
            IJson data = new GroupDto()
            {
                GroupID = GroupID
            };
            HttpResponseMessage response = _http.Delete(baseUrl + url, data).Result;
            if (!response.IsSuccessStatusCode)
            {
                GroupID = 0;
                return;
            }
            _nav.NavigateTo("groups");
        }

        public void OwnerRemovesUser(User user)
        {
            RemoveParticipant(user);
            string url = "RemoveParticipant";
            IJson data = new GroupUserDto()
            {
                GroupRequest = new GroupDto()
                {
                    GroupID = GroupID
                },
                UserRequest = new UserDto()
                {
                    UserID = user.UserID
                }
            };
            HttpResponseMessage response = _http.Post(baseUrl + url, data).Result;
            if (!response.IsSuccessStatusCode)
            {
                string error = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(error);
                return;
            }
            StateHasChanged();
        }

        public void OwnerInvitesUsers(User user) //
        {
            AddParticipant(user);
            string url = "AddParticipant";
            IJson data = new GroupEmailDto()
            {
                GroupRequest = new GroupDto()
                {
                    GroupID = GroupID
                },
                EmailRequest = new EmailDto()
                {
                    Email = user.Email
                    
                }
            };
            HttpResponseMessage response = _http.Put(baseUrl + url, data).Result;
            if (!response.IsSuccessStatusCode)
            {
                string error = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(error);
                return;
            }
            StateHasChanged();
        }
    }
}
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
        public string Email { get; set; }
        
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
            string url = "UpdateGroup";
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

        public void OwnerAddsUsers(User user) //
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
                    Email = Email
                    
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
        public void UserLeavesGroup() //Method that allows a user to leave the group, if they are not the owner.
        {
            string url = "LeaveGroup";
            IJson data = new GroupDto()
            {
                GroupID = GroupID
            };
            HttpResponseMessage response = _http.Put(baseUrl + url, data).Result;
            if (!response.IsSuccessStatusCode)
            {
                GroupID = 0;
                return;
            }
            _nav.NavigateTo("groups");
        }

        public void UserDeletesActivity(Activity activity)
        {
            string url = "Delete";
            IJson data = new ActivityDto()
            {
                ActivityID = activity.ActivityID
            };
            HttpResponseMessage response = _http.Delete(url, data).Result;
            if (!response.IsSuccessStatusCode)
            {
                GroupID = 0;
                return;
            }
            ReadGroupData();
        }
        public void UserEditsActivity(Activity activity)
        {
            string url = "UpdateActivity";
            IJson data = new Activity()
            {
                ActivityID = activity.ActivityID,
                Group = activity.Group,
                Amount = activity.Amount,
                Description = activity.Description,
            };
            HttpResponseMessage response = _http.Put(url, data).Result;
            if (!response.IsSuccessStatusCode)
            {
                GroupID = 0;
                return;
            }
            ReadGroupData();
        }
        public void UserAddsActivity(Activity activity)
        {
            string url = "Create";
            IJson data = new GroupDto()
            {
                GroupID = GroupID
            };
            HttpResponseMessage response = _http.Post(url, data).Result;
            if (!response.IsSuccessStatusCode)
            {
                GroupID = 0;
                return;
            }
            ReadGroupData();
        }
        public void UserReadsActivity(Activity activity)
        {
            string url = "ReadOne";
            IJson data = new ActivityDto()
            {
                ActivityID = activity.ActivityID
            };
            HttpResponseMessage response = _http.Post(url, data).Result;
            if (!response.IsSuccessStatusCode)
            {
                GroupID = 0;
                return;
            }
            ReadGroupData();
        }
        public void OwnerReadsAllActivities()
        {
            string url = "ReadMany";
            IJson data = new GroupDto()
            {
                GroupID = GroupID
            };
            HttpResponseMessage response = _http.Post(url, data).Result;
            if (!response.IsSuccessStatusCode)
            {
                GroupID = 0;
                return;
            }
            ReadGroupData();
        }
    }
}
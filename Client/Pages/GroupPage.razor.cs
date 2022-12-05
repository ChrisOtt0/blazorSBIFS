﻿using blazorSBIFS.Shared.DataTransferObjects;
using blazorSBIFS.Shared.Models;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace blazorSBIFS.Client.Pages
{
    public partial class GroupPage
    {
        string baseUrl = "Group/";

        [Parameter]
        public int GroupID { get; set; }
        public string GroupName { get; set; } = "not found.";
        public Group? Group { get; set; }

        /// <summary>
        /// It gets the group from the database and sets the GroupName to the group's name
        /// </summary>
        /// <returns>
        /// A Group object.
        /// </returns>
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
        public void AddParticipant(User user) //Add participants method is not implemented yet into the .razor file
        {
            Group.Participants.Add(user);
            StateHasChanged();
        }
        /// <summary>
        /// The function is called when the user clicks the save button. It sends a post request to the server with the
        /// group's name and participants. If the request is successful, the group's name is updated
        /// </summary>
        /// <returns>
        /// The method returns a list of all the groups that the user is a part of.
        /// </returns>
        public void SaveChanges() //Add participants method is not implemented yet into the .razor file
        {
            string url = "Update";
            IJson data = new GroupDto
            {
                GroupID = GroupID,
                Name = Group.Name,
                Participants = Group.Participants
            };
            HttpResponseMessage response = _http.Post(baseUrl + url, data).Result;
            if (!response.IsSuccessStatusCode)
            {
                GroupID = 0;
                return;
            }
            UpdateName();
    }
        /// <summary>
        /// It reads the group data from the server and updates the name of the group
        /// </summary>
        /// <returns>
        /// A Group object.
        /// </returns>
        public void ReadGroupData()
        {
            string url = "ReadOne";
            IJson data = new GroupDto
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
        /// <summary>
        /// The function takes the GroupID from the view model and sends it to the server. If the server returns a success
        /// code, the user is navigated to the groups page
        /// </summary>
        /// <returns>
        /// A HttpResponseMessage
        /// </returns>
        public void DeleteGroup()
        {
            string url = "Delete";
            IJson data = new GroupDto
            {
                GroupID = GroupID
            };
            HttpResponseMessage response = _http.Post(baseUrl + url, data).Result;
            if (!response.IsSuccessStatusCode)
            {
                GroupID = 0;
                return;
            }
            _nav.NavigateTo("groups");
        }
        public void OwnerInvitesUsers()
        {
            _nav.NavigateTo("invite/" + GroupID);
        }
        public void OwnerRemovesUsers()
        {
            _nav.NavigateTo("remove/" + GroupID);
        }
        public void OwnerRemovesUser(User user)
        {
            RemoveParticipant(user);
            string url = "RemoveUser";
            IJson data = new GroupDto
            {
                GroupID = GroupID,
                Participants = new List<User> { user }
            };
            HttpResponseMessage response = _http.Post(baseUrl + url, data).Result;
            if (!response.IsSuccessStatusCode)
            {
                GroupID = 0;
                return;
            }
            StateHasChanged();
        }
    }
}
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
        public void SaveChanges() //Add participants method is not implemented yet into the .razor file
        {
            string url = "Update";
            IJson data = new GroupDto
            {
                GroupID = this.GroupID,
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
}
}
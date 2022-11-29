using blazorSBIFS.Client.Pages;
using blazorSBIFS.Client.Services;
using blazorSBIFS.Shared.Models;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;
using System.Net.Http.Json;

namespace blazorSBIFS.Client.ViewModels
{
    public interface IGroupsViewModel
    {
        public List<Group> Groups { get; set; }
        public string BaseUrl { get; }
        public Action StateHasChangedDelegate { get; set; }
        public void NewGroup();
    }

    public class GroupsViewModel : IGroupsViewModel
    {
        private readonly IHttpService _httpService;
        public string BaseUrl { get; } = "Group/";

        public List<Group> Groups { get; set; } = new List<Group>();
        public Action StateHasChangedDelegate { get; set; }

        public GroupsViewModel(IHttpService httpService)
        {
            _httpService = httpService;
        }

        public async void NewGroup()
        {
            string url = "Create";
            HttpResponseMessage response = await _httpService.Post(BaseUrl + url, null);

            // catching API errors
            if (!response.IsSuccessStatusCode) return;

            Groups = await response.Content.ReadFromJsonAsync<List<Group>>();
            StateHasChangedDelegate();
        }
    }
}

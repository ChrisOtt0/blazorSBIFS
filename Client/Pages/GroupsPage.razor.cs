using blazorSBIFS.Shared.Models;
using System.Net.Http.Json;
using blazorSBIFS.Shared.DataTransferObjects;

namespace blazorSBIFS.Client.Pages
{
    public partial class GroupsPage
    {
        string baseUrl = "Group/";

        public List<Group> Groups { get; set; } = new List<Group>();

        protected override async void OnInitialized()
        {
            if (_token.Jwt == string.Empty) _nav.NavigateTo("login");

            string url = "ReadMany";
            HttpResponseMessage response = await _http.Get(baseUrl + url);

            // catching errors
            if (!response.IsSuccessStatusCode) return;
            if (!((int)response.StatusCode == 200)) return;

            List<Group>? groups = await response.Content.ReadFromJsonAsync<List<Group>>();
            if (groups == null) return;

            Groups = groups;
            StateHasChanged();
        }

        public async void NewGroup()
        {
            string url = "Create";
            HttpResponseMessage response = await _http.Post(baseUrl + url, null);

            // catching API errors
            if (!response.IsSuccessStatusCode) return;

            Groups = await response.Content.ReadFromJsonAsync<List<Group>>();
            StateHasChanged();
        }
    }
}
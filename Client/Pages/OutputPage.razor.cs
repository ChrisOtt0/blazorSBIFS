using blazorSBIFS.Shared.DataTransferObjects;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace blazorSBIFS.Client.Pages
{
	public partial class OutputPage
	{
		[Parameter]
		public int GroupID { get; set; }
		public string? Calculation { get; set; }

		protected override async void OnInitialized()
		{
			if (GroupID == 0)
			{
				Calculation = "Unexpected error.";
				StateHasChanged();
				return;
			}

			string url = "Group/Calculate";
			IJson data = new GroupCalculateDto
			{
				GroupID = this.GroupID,
				OutputForm = OutputForm.html
			};

			HttpResponseMessage response = await _http.Post(url, data);
			if (!response.IsSuccessStatusCode)
			{
				Calculation = ((int)response.StatusCode).ToString()
					+ ": " + await response.Content.ReadAsStringAsync();
				StateHasChanged();
				return;
			}

			CalculationDto? result = await response.Content.ReadFromJsonAsync<CalculationDto>();
			if (result == null || result.Html == null)
			{
				Calculation = "Unexpected error.";
				StateHasChanged();
				return;
			}

			Console.WriteLine("EndReached");
			Calculation = result.Html;
			StateHasChanged();
		}
	}
}

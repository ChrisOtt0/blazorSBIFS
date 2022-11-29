using blazorSBIFS.Client.Extensions;
using blazorSBIFS.Shared.DataTransferObjects;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace blazorSBIFS.Client.Services
{
	public interface IHttpService
	{
        public Task<HttpResponseMessage> Get(string url);
        public Task<HttpResponseMessage> Post(string url, IJson data);
        public Task<HttpResponseMessage> Put(string url, IJson data);
        public Task<HttpResponseMessage> Delete(string url, IJson data);
        public void AddAuthorization(string jwt);
    }

	public class HttpService : IHttpService
	{
        HttpClient client = new HttpClient();
        string baseUrl = "https://localhost:7120/api/";

        public async Task<HttpResponseMessage> Get(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, baseUrl + url);
            return await client.GetAsync(baseUrl + url);
        }

        public async Task<HttpResponseMessage> Post(string url, IJson data)
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(baseUrl + url),
                Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
            };
            return await client.SendAsync(request);
        }

        public async Task<HttpResponseMessage> Put(string url, IJson data)
        {
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(baseUrl + url),
                Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
            };
            return await client.SendAsync(request);
        }

        public async Task<HttpResponseMessage> Delete(string url, IJson data)
        {
            return await HttpClientExtension.DeleteAsJsonAsync(client, baseUrl + url, data);
        }

        public void AddAuthorization(string jwt)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        }
    }
}

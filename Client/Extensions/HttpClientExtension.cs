using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace blazorSBIFS.Client.Extensions
{
    public static class HttpClientExtension
    {
        public static Task<HttpResponseMessage> DeleteAsJsonAsync<T>(this HttpClient httpClient, string requestUri, T data)
        => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, requestUri) { Content = Serialize(data) });

        public static Task<HttpResponseMessage> DeleteAsJsonAsync<T>(this HttpClient httpClient, string requestUri, T data, CancellationToken cancellationToken)
            => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, requestUri) { Content = Serialize(data) }, cancellationToken);

        public static Task<HttpResponseMessage> DeleteAsJsonAsync<T>(this HttpClient httpClient, Uri requestUri, T data)
            => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, requestUri) { Content = Serialize(data) });

        public static Task<HttpResponseMessage> DeleteAsJsonAsync<T>(this HttpClient httpClient, Uri requestUri, T data, CancellationToken cancellationToken)
            => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, requestUri) { Content = Serialize(data) }, cancellationToken);

        private static HttpContent Serialize(object data) => new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
    }
}

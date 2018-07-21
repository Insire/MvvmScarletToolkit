using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.IconManager
{
    public sealed class PublicApis : IDisposable
    {
        private const string BaseUrl = "https://api.publicapis.org/";
        private const string UserAgentHeaderName = "User-Agent";
        private const string UserAgentHeaderValue = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";
        private readonly bool _shouldDisposeHttpClient;

        private HttpClient _httpClient;
        private bool _disposed;

        public PublicApis(HttpClient httpClient, bool shouldDisposeHttpClient)
        {
            _shouldDisposeHttpClient = shouldDisposeHttpClient;
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            _httpClient.BaseAddress = new Uri(BaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Add(UserAgentHeaderName, UserAgentHeaderValue);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public PublicApis(HttpClient httpClient)
            : this(httpClient, false)
        {
        }

        public PublicApis()
            : this(new HttpClient(), true)
        {
        }

        public async Task<CategoryResponse> GetCategories(CancellationToken token)
        {
            var response = await _httpClient.GetAsync("/categories", token).ConfigureAwait(false);
            var data = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<CategoryResponse>(data);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_shouldDisposeHttpClient)
                    {
                        _httpClient.Dispose();
                        _httpClient = null;
                    }
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }
    }
}

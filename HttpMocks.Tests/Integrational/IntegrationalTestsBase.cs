using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NUnit.Framework;

namespace HttpMocks.Tests.Integrational
{
    public class IntegrationalTestsBase
    {
        protected HttpMockRepository HttpMocks;

        [SetUp]
        public void SetUp()
        {
            HttpMocks = new HttpMockRepository();
        }

        [TearDown]
        public virtual void TearDown()
        {
            HttpMocks.VerifyAll();
        }

        protected readonly Uri DefaultMockUrl = new Uri("http://localhost:3465/");

        protected static Uri BuildUrl(Uri mockUrl, string path, NameValueCollection query = null)
        {
            var uriBuilder = new UriBuilder(mockUrl.Scheme, mockUrl.Host, mockUrl.Port, path);

            if (query != null)
            {
                uriBuilder.Query = string.Join("&", query.AllKeys.Select(x => $"{x}={query[x]}"));
            }

            return uriBuilder.Uri;
        }

        protected static async Task<TestResponse> SendAsync(Uri url, HttpMethod method, byte[] contentBytes = null,
            string contentType = null, NameValueCollection headers = null)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(method, url);

                if (headers != null)
                {
                    foreach (var headerName in headers.AllKeys)
                    {
                        request.Headers.Add(headerName, headers[headerName]);
                    }
                }

                if (contentBytes is { Length: > 0 })
                {
                    request.Content = new ByteArrayContent(contentBytes);
                    if (contentType != null)
                    {
                        request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                    }
                }

                client.Timeout = TimeSpan.FromMilliseconds(2000);

                var httpWebResponse = await client.SendAsync(request);
                return await ConvertAsync(httpWebResponse);
            }
            catch (HttpRequestException)
            {
                return TestResponse.Create(452);
            }
        }

        private static async Task<TestResponse> ConvertAsync(HttpResponseMessage httpWebResponse)
        {
            return TestResponse.Create((int)httpWebResponse.StatusCode,
                await ReadResponseContentBytesAsync(httpWebResponse));
        }

        private static async Task<byte[]> ReadResponseContentBytesAsync(HttpResponseMessage httpWebResponse)
        {
            return await httpWebResponse.Content.ReadAsByteArrayAsync();
        }
    }
}
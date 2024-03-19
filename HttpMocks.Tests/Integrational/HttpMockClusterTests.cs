using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace HttpMocks.Tests.Integrational
{
    [TestFixture]
    public class HttpMockClusterTests : IntegrationalTestsBase
    {
        [Test]
        public async Task TestSuccess()
        {
            IHttpMock httpMock1;
            using (httpMock1 = HttpMocks.NewCluster("localhost", 2))
            {
                httpMock1
                    .WhenRequestGet("/bills/1")
                    .ThenResponse(200);
            }

            (await SendAsync(BuildUrl(httpMock1.MockUri, "/bills/1"), HttpMethod.Get)).StatusCode.ShouldBeEquivalentTo(200);
        }
    }
}
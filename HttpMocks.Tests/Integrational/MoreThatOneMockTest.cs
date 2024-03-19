using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using HttpMocks.Exceptions;
using NUnit.Framework;

namespace HttpMocks.Tests.Integrational
{
    [TestFixture]
    public class MoreThatOneMockTest : IntegrationalTestsBase
    {
        public override void TearDown()
        {
        }

        [Test]
        public async Task TestSuccess()
        {
            IHttpMock httpMock1;
            using (httpMock1 = HttpMocks.New("localhost", 3465))
            {
                httpMock1
                    .WhenRequestGet("/bills/1")
                    .ThenResponse(200);
            }

            IHttpMock httpMock2;
            using (httpMock2 = HttpMocks.New("localhost", 3466))
            {
                httpMock2
                    .WhenRequestGet("/bills/2")
                    .ThenResponse(200);
            }

            (await SendAsync(BuildUrl(httpMock1.MockUri, "/bills/1"), HttpMethod.Get)).StatusCode.ShouldBeEquivalentTo(200);
            (await SendAsync(BuildUrl(httpMock2.MockUri, "/bills/2"), HttpMethod.Get)).StatusCode.ShouldBeEquivalentTo(200);

            HttpMocks.VerifyAll();
        }

        [Test]
        public async Task TestNotActualWhenRepeatCountMoreActualCount()
        {
            IHttpMock httpMock1;
            using (httpMock1 = HttpMocks.New("localhost", 3465))
            {
                httpMock1
                    .WhenRequestGet("/bills/1")
                    .ThenResponse(200)
                    .Repeat(2);
            }

            (await SendAsync(BuildUrl(httpMock1.MockUri, "/bills/1"), HttpMethod.Get)).StatusCode.ShouldBeEquivalentTo(200);

            HttpMocks.Invoking(x => x.VerifyAll()).ShouldThrow<AssertHttpMockException>();
        }

        [Test]
        public async Task TestNotActualWhenNotExpected()
        {
            IHttpMock httpMock1;
            using (httpMock1 = HttpMocks.New("localhost", 3465))
            {
                httpMock1
                    .WhenRequestGet("/bills/1")
                    .ThenResponse(200);

                httpMock1
                    .WhenRequestGet("/bills/2")
                    .ThenResponse(200);
            }

            (await SendAsync(BuildUrl(httpMock1.MockUri, "/bills/1"), HttpMethod.Get)).StatusCode.ShouldBeEquivalentTo(200);

            HttpMocks.Invoking(x => x.VerifyAll()).ShouldThrow<AssertHttpMockException>();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using HttpMocks.Exceptions;
using HttpMocks.Implementation;
using HttpMocks.Whens;
using NUnit.Framework;

namespace HttpMocks.Tests.Integrational
{
    [TestFixture]
    public class MethodAndPathTests : IntegrationalTestsBase
    {
        [TearDown]
        public override void TearDown()
        {
        }

        [Test]
        public async Task TestSuccessWhenGetReturn302()
        {
            using (var httpMock = HttpMocks.New(DefaultMockUrl))
            {
                httpMock
                    .WhenRequestGet("/bills")
                    .ThenResponse(302);
            }

            var url = BuildUrl(DefaultMockUrl, "/bills");
            var response = await SendAsync(url, HttpMethod.Get);

            response.StatusCode.ShouldBeEquivalentTo(302);
            response.ContentBytes.Length.ShouldBeEquivalentTo(0);

            HttpMocks.VerifyAll();
        }

        [Test]
        public async Task TestSuccessWhenHeadersDefined()
        {
            var headers = new NameValueCollection { { "X-Header-Name", "Header_Value" } };
            using (var httpMock = HttpMocks.New(DefaultMockUrl))
            {
                httpMock
                    .WhenRequestGet("/")
                    .Headers(headers)
                    .ThenResponse(200);
            }

            var url = BuildUrl(DefaultMockUrl, "/");
            var responseA = await SendAsync(url, HttpMethod.Get);

            responseA.StatusCode.ShouldBeEquivalentTo(500);

            var responseB = await SendAsync(url, HttpMethod.Get, headers: headers);

            responseB.StatusCode.ShouldBeEquivalentTo(200);

            HttpMocks.Invoking(m => m.VerifyAll())
                .ShouldThrowExactly<AssertHttpMockException>()
                .WithMessage("Actual request GET /, but not expected.");
        }

        [Test]
        public async Task TestSuccessWhenQueryDefined()
        {
            var query = new NameValueCollection { { "qp", "qv" } };
            using (var httpMock = HttpMocks.New(DefaultMockUrl))
            {
                httpMock
                    .WhenRequestGet("/")
                    .Query(query)
                    .ThenResponse(200);
            }

            var responseA = await SendAsync(BuildUrl(DefaultMockUrl, "/"), HttpMethod.Get);

            responseA.StatusCode.ShouldBeEquivalentTo(500);

            var responseB = await SendAsync(BuildUrl(DefaultMockUrl, "/", query), HttpMethod.Get);

            responseB.StatusCode.ShouldBeEquivalentTo(200);

            HttpMocks.Invoking(m => m.VerifyAll())
                .ShouldThrowExactly<AssertHttpMockException>()
                .WithMessage("Actual request GET /, but not expected.");
        }

        [Test]
        public async Task TestFailWhenActualRepeatMoreThatExpected()
        {
            using (var httpMock = HttpMocks.New(DefaultMockUrl))
            {
                httpMock
                    .WhenRequestGet("/bills")
                    .ThenResponse(200)
                    .Repeat(1);
            }

            var url = BuildUrl(DefaultMockUrl, "/bills");

            (await SendAsync(url, HttpMethod.Get)).StatusCode.ShouldBeEquivalentTo(200);

            (await SendAsync(url, HttpMethod.Get)).StatusCode.ShouldBeEquivalentTo(500);

            HttpMocks.Invoking(m => m.VerifyAll())
                .ShouldThrowExactly<AssertHttpMockException>()
                .WithMessage("Actual request GET /bills repeat count 2, but max expected repeat count 1.");
        }

        [Test]
        public async Task TestFailWhenDefaultActualRepeatMoreThatExpected()
        {
            using (var httpMock = HttpMocks.New(DefaultMockUrl))
            {
                httpMock
                    .WhenRequestGet("/bills")
                    .ThenResponse(200);
            }

            var url = BuildUrl(DefaultMockUrl, "/bills");

            (await SendAsync(url, HttpMethod.Get)).StatusCode.ShouldBeEquivalentTo(200);

            (await SendAsync(url, HttpMethod.Get)).StatusCode.ShouldBeEquivalentTo(500);

            HttpMocks.Invoking(m => m.VerifyAll())
                .ShouldThrowExactly<AssertHttpMockException>()
                .WithMessage("Actual request GET /bills repeat count 2, but max expected repeat count 1.");
        }

        [Test]
        public async Task TestFailWhenAnyActualRepeatMoreThatExpected()
        {
            using (var httpMock = HttpMocks.New(DefaultMockUrl))
            {
                httpMock
                    .WhenRequestGet("/bills")
                    .ThenResponse(200)
                    .RepeatAny();
            }

            var url = BuildUrl(DefaultMockUrl, "/bills");

            (await SendAsync(url, HttpMethod.Get)).StatusCode.ShouldBeEquivalentTo(200);

            (await SendAsync(url, HttpMethod.Get)).StatusCode.ShouldBeEquivalentTo(200);

            HttpMocks.VerifyAll();
        }

        [Test]
        public async Task TestSuccessWhenGetReturn200AndResult()
        {
            const string testDataString = "Test data";
            var content = Encoding.UTF8.GetBytes(testDataString);

            using (var httpMock = HttpMocks.New(DefaultMockUrl))
            {
                httpMock
                    .WhenRequestGet("/bills")
                    .ThenResponse(200)
                    .Content(content, "text/plain");
            }

            var url = BuildUrl(DefaultMockUrl, "/bills");
            var response = await SendAsync(url, HttpMethod.Get);

            response.StatusCode.ShouldBeEquivalentTo(200);
            response.ContentBytes.Length.ShouldBeEquivalentTo(content.Length);
            Encoding.UTF8.GetString(response.ContentBytes).ShouldBeEquivalentTo(testDataString);

            HttpMocks.VerifyAll();
        }

        [Test]
        public async Task TestFailWhenActualIsNotExpectedRequest()
        {
            using (var httpMock = HttpMocks.New(DefaultMockUrl))
            {
                httpMock
                    .WhenRequestGet("/bills/@guid")
                    .ThenResponse(302);
            }

            var url = BuildUrl(DefaultMockUrl, "/bills");
            var response = await SendAsync(url, HttpMethod.Get);

            response.StatusCode.ShouldBeEquivalentTo(500);
            response.ContentBytes.Length.ShouldBeEquivalentTo(0);

            HttpMocks
                .Invoking(m => m.VerifyAll())
                .ShouldThrowExactly<AssertHttpMockException>();
        }

        [Test]
        public async Task TestSuccessWhenResponseFromDelegate()
        {
            var paths = new List<string>();

            Func<HttpRequest, HttpResponse> processRequestInfo = request =>
            {
                paths.Add(request.Path);
                return HttpResponse.Create(200);
            };

            using (var httpMock = HttpMocks.New(DefaultMockUrl))
            {
                httpMock
                    .WhenRequestGet("/bills/@guid")
                    .ThenResponse(i => processRequestInfo(i));
            }

            var guid = Guid.NewGuid();
            var url = BuildUrl(DefaultMockUrl, $"/bills/{guid}");
            var response = await SendAsync(url, HttpMethod.Get);

            response.StatusCode.ShouldBeEquivalentTo(200);
            response.ContentBytes.Length.ShouldBeEquivalentTo(0);

            paths.Count.ShouldBeEquivalentTo(1);
            paths[0].ShouldBeEquivalentTo($"/bills/{guid}");

            HttpMocks.VerifyAll();
        }

        [Test]
        public async Task TestSuccessWhenResponseFromAsyncDelegate()
        {
            var paths = new List<string>();

            Func<HttpRequest, Task<HttpResponse>> processRequestInfoAsync = request =>
            {
                paths.Add(request.Path);
                return Task.FromResult(HttpResponse.Create(200));
            };

            using (var httpMock = HttpMocks.New(DefaultMockUrl))
            {
                httpMock
                    .WhenRequestGet("/bills/@guid")
                    .ThenResponse(i => processRequestInfoAsync(i));
            }

            var guid = Guid.NewGuid();
            var url = BuildUrl(DefaultMockUrl, $"/bills/{guid}");
            var response = await SendAsync(url, HttpMethod.Get);

            response.StatusCode.ShouldBeEquivalentTo(200);
            response.ContentBytes.Length.ShouldBeEquivalentTo(0);

            paths.Count.ShouldBeEquivalentTo(1);
            paths[0].ShouldBeEquivalentTo($"/bills/{guid}");

            HttpMocks.VerifyAll();
        }
    }
}
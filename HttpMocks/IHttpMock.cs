using System;
using System.Runtime.CompilerServices;
using HttpMocks.Whens;
using HttpMocks.Whens.RequestPatterns;

[assembly: InternalsVisibleTo("HttpMocks.Tests")]

namespace HttpMocks
{
    public interface IHttpMock : IDisposable
    {
        IHttpRequestMock WhenRequestGet(IHttpRequestPathPattern pathPattern);
        IHttpRequestMock WhenRequestPost(IHttpRequestPathPattern pathPattern);
        IHttpRequestMock WhenRequestPut(IHttpRequestPathPattern pathPattern);
        IHttpRequestMock WhenRequestDelete(IHttpRequestPathPattern pathPattern);
        IHttpRequestMock WhenRequestPatch(IHttpRequestPathPattern pathPattern);
        IHttpRequestMock WhenRequest();

        void Run();

        Uri[] MockUris { get; }
        Uri MockUri { get; }
    }
}
using System;
using ServiceStack.LocalRequest.Contracts;
using Microsoft.Extensions.Logging;

namespace ServiceStack.LocalRequest.Client
{
    public class SimpleHttpJsonRestClient : SimpleHttpRestClient
    {
        public SimpleHttpJsonRestClient(Func<SimpleHttpRequest, SimpleHttpResponse> sendFunc
            , ILogger logger) : base(sendFunc, logger)
        {
            Format = "json";
        }
    }
}
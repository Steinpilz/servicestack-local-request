using System;
using ServiceStack.LocalRequest.Contracts;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ServiceStack.LocalRequest.Client
{
    public class SimpleHttpJsonRestClient : SimpleHttpRestClient
    {
        public SimpleHttpJsonRestClient(
            Func<SimpleHttpRequest, SimpleHttpResponse> sendFunc
            , ILogger logger) : base(sendFunc, logger)
        {
            Format = "json";
        }

        public SimpleHttpJsonRestClient(
            Func<SimpleHttpRequest, Task<SimpleHttpResponse>> sendFuncAsync
            , ILogger logger) : base(sendFuncAsync, logger)
        {
            Format = "json";
        }
    }
}
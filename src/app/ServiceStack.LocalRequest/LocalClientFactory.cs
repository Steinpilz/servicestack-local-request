using System;
using ServiceStack.LocalRequest.Client;
using ServiceStack.LocalRequest.Contracts;
using Microsoft.Extensions.Logging;

namespace ServiceStack.LocalRequest
{
    public class LocalClientFactory
    {
        readonly Func<SimpleRequestExecutor> _executorFunc;
        readonly ILogger logger;

        public LocalClientFactory(Func<SimpleRequestExecutor> executorFunc, ILogger logger)
        {
            this.logger = logger;
            _executorFunc = executorFunc ?? throw new ArgumentNullException(nameof(executorFunc));
        }

        public SimpleHttpJsonRestClient CreateJsonClient()
        {
            return new SimpleHttpJsonRestClient(SendRequest, logger);
        }

        private SimpleHttpResponse SendRequest(SimpleHttpRequest request)
        {
            var executor = _executorFunc();
            var response = executor.Execute(request);
            return response;
        }
    }
}
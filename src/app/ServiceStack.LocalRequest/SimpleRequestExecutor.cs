using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.LocalRequest.Contracts;
using ServiceStack.Text;
using Microsoft.Extensions.Logging;

namespace ServiceStack.LocalRequest
{
    public class SimpleRequestExecutor
    {
        private readonly LocalServiceStackHost _host;
        private readonly bool _logRequests;

        public int RetriesCount { get; set; } = 10;

        readonly ILogger logger;

        public SimpleRequestExecutor(
            LocalServiceStackHost host, 
            bool logRequests, 
            ILogger logger)
        {
            this.logger = logger;
            _logRequests = logRequests;
            _host = host ?? throw new ArgumentNullException(nameof(host));
        }

        public SimpleHttpResponse Execute(SimpleHttpRequest request)
        {
            var clientRequestId = ExtractClientRequestId(request);
            var requestId = Guid.NewGuid().ToString("N");

            if (_logRequests)
                logger.LogDebug($"Request [{clientRequestId}] [tmp-{requestId}]: {request.Dump()}");

            SimpleHttpResponse response = null;

            for (var i = 0; i < RetriesCount; i++)
            {
                response = new SimpleHttpResponse
                {
                    Headers = new List<HttpHeader>(),
                };

                var httpReq = new SimpleHttpRequestAdapter(request);
                var httpRes = new SimpleHttpResponseAdapter(response);
                _host.Handle(httpReq, httpRes);

                if (!httpReq.Items.ContainsKey("RetryRequest"))
                    break;
            }

            if (_logRequests)
                logger.LogDebug($"Response [{clientRequestId}] [tmp-{requestId}]: {response.Dump()}");

            return response;
        }

        private string ExtractClientRequestId(SimpleHttpRequest request)
        {
            return request.Headers?.FirstOrDefault(x => x.Key == "X-Request-Id")?.Value;
        }
    }
}
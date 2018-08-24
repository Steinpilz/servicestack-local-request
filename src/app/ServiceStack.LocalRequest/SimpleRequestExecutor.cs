using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.LocalRequest.Contracts;
using ServiceStack.Text;
using Microsoft.Extensions.Logging;
using ServiceStack.LocalRequest.Debug;

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

        public ExecutionResult Execute(ExecutionContext context)
        {
            var request = context.Request;
            var clientRequestId = ExtractClientRequestId(request);
            var requestId = Guid.NewGuid().ToString("N");

            if (_logRequests)
                logger.LogDebug($"Request [{clientRequestId}] [tmp-{requestId}]:\r\n {new RequestDumper(request).Dump()}");

            SimpleHttpResponse response = null;
            for (var i = 0; i < RetriesCount; i++)
            {
                response = new SimpleHttpResponse
                {
                    Headers = new List<HttpHeader>(),
                };

                var httpReq = new SimpleHttpRequestAdapter(request);
                SetupProperties(context, httpReq);

                var httpRes = new SimpleHttpResponseAdapter(response);

                if (!_host.Handle(httpReq, httpRes))
                    return new ExecutionResult(response, false);

                if (!httpReq.Items.ContainsKey("RetryRequest"))
                    break;
            }

            if (_logRequests)
                logger.LogDebug($"Response [{clientRequestId}] [tmp-{requestId}]:\r\n {new ResponseDumper(response).Dump()}");

            return new ExecutionResult(response, true);
        }

        private static void SetupProperties(ExecutionContext context, SimpleHttpRequestAdapter request)
        {
            if (context.Properties == null)
                return;

            foreach (var item in context.Properties)
                request.Items[item.Key] = item.Value;
        }

        public bool TryExecute(SimpleHttpRequest request, out SimpleHttpResponse response)
        {
            var result = this.Execute(new ExecutionContext(request, null));
            response = result.Response;
            return result.HandlerFound;
        }

        public SimpleHttpResponse Execute(SimpleHttpRequest request)
        {
            TryExecute(request, out SimpleHttpResponse response);
            return response;
        }

        private string ExtractClientRequestId(SimpleHttpRequest request)
        {
            return request.Headers?.FirstOrDefault(x => x.Key == "X-Request-Id")?.Value;
        }
    }
}
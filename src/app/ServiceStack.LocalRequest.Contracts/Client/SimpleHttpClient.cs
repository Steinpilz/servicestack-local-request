using System;
using System.Collections.Generic;
using ServiceStack.Common.Web;
using ServiceStack.LocalRequest.Contracts;
using System.Linq;
using ServiceStack.Text;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ServiceStack.LocalRequest.Client
{
    public class SimpleHttpClient 
    {
        public string ContentType => $"application/{Format}";

        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        public string Format { get; set; }
        public bool LogRequests { get; set; }
        public string UrlPrefix { get; set; }

        readonly Func<SimpleHttpRequest, SimpleHttpResponse> sendFunc;
        readonly Func<SimpleHttpRequest, Task<SimpleHttpResponse>> sendFuncAsync;
        readonly ILogger logger;

        protected SimpleHttpClient(
            Func<SimpleHttpRequest, Task<SimpleHttpResponse>> sendFuncAsync,
            ILogger logger
            )
        {
            this.logger = logger;
            this.sendFuncAsync = sendFuncAsync;
            this.sendFunc = req =>
            {
                var task = this.sendFuncAsync(req);
                task.ConfigureAwait(false);
                return task.Result;
            };
        }

        protected SimpleHttpClient(
            Func<SimpleHttpRequest, SimpleHttpResponse> sendFunc, 
            ILogger logger
            )
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.sendFunc = sendFunc ?? throw new ArgumentNullException(nameof(sendFunc));
            this.sendFuncAsync = req => Task.Factory.StartNew(() => this.sendFunc(req));
        }

        protected SimpleHttpResponse SendRequest(SimpleHttpRequest request)
        {
            if (string.IsNullOrEmpty(Format))
                throw new InvalidOperationException("Format Property must be not empty");

            return sendFunc(request);
        }

        protected async Task<SimpleHttpResponse> SendRequestAsync(SimpleHttpRequest request)
        {
            if (string.IsNullOrEmpty(Format))
                throw new InvalidOperationException("Format Property must be not empty");

            return await sendFuncAsync(request).ConfigureAwait(false);
        }

        public SimpleHttpResponse Send(SimpleHttpRequest request)
        {
            AddHeadersToRequest(request);
            var requestId = AddCustomRequestIdHeader(request);
            
            if(LogRequests)
                logger.LogDebug($"Sending request [{requestId}] {request.Dump()}");
            
            var result = SendRequest(request);

            if(LogRequests)
                logger.LogDebug($"Got response [{requestId}] {result.Dump()}");
                
            return result;
        }

        public async Task<SimpleHttpResponse> SendAsync(SimpleHttpRequest request)
        {
            AddHeadersToRequest(request);
            var requestId = AddCustomRequestIdHeader(request);

            if (LogRequests)
                logger.LogDebug($"Sending request [{requestId}] {request.Dump()}");

            var result = await SendRequestAsync(request).ConfigureAwait(false);

            if (LogRequests)
                logger.LogDebug($"Got response [{requestId}] {result.Dump()}");

            return result;
        }

        void AddHeadersToRequest(SimpleHttpRequest request)
        {
            if (!Headers.ContainsKey(HttpHeaders.Accept))
            {
                Headers[HttpHeaders.Accept] = ContentType;
            }
            if (!Headers.ContainsKey(HttpHeaders.ContentType))
            {
                Headers[HttpHeaders.ContentType] = ContentType;
            }
            Headers[HttpHeaders.ContentLength] = (request.Body?.Length ?? 0).ToString();
            
            if(request.Headers == null)
                request.Headers = new List<HttpHeader>();
            
            foreach(var header in Headers)
            {
                if(request.Headers.Any(x => string.Equals(x.Key, header.Key, StringComparison.CurrentCultureIgnoreCase)))
                    continue;
                    
                request.Headers.Add(new HttpHeader {Key = header.Key, Value = header.Value});
            }
        }
        
        string AddCustomRequestIdHeader(SimpleHttpRequest request)
        {
            var requestId = Guid.NewGuid().ToString("N");
            request.Headers.Add(new HttpHeader{Key = "X-Request-Id", Value = requestId });
            return requestId;
        }
    }
}
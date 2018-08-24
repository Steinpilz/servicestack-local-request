using ServiceStack.LocalRequest.Contracts;
using System.Collections.Generic;

namespace ServiceStack.LocalRequest
{
    public class ExecutionContext
    {
        public Dictionary<string, object> Properties { get; }
        public SimpleHttpRequest Request { get; }

        public ExecutionContext(
            SimpleHttpRequest request,
            Dictionary<string, object> properties)
        {
            Properties = properties;
            Request = request;
        }
    }
}
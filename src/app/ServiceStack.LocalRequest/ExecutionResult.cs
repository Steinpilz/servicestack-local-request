using ServiceStack.LocalRequest.Contracts;

namespace ServiceStack.LocalRequest
{
    public class ExecutionResult
    {
        public SimpleHttpResponse Response { get; }
        public bool HandlerFound { get; }

        public ExecutionResult(SimpleHttpResponse response, bool handlerFound)
        {
            Response = response;
            HandlerFound = handlerFound;
        }
    }
}
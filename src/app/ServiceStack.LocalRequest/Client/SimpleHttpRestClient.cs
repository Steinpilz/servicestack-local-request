using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using ServiceStack.LocalRequest.Contracts;
using ServiceStack.Service;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceModel.Serialization;
using ServiceStack.Text;
using Microsoft.Extensions.Logging;

namespace ServiceStack.LocalRequest.Client
{
    public class SimpleHttpRestClient : SimpleHttpClient, IRestClient
    {
        protected TResponse SendAndDeserialize<TResponse>(string method, IReturn<TResponse> request, string url = null)
        {
            var requestUrl = request.ToUrl(method, Format);
            var response = SendInternal(method, request, url ?? requestUrl);

            AssertResponse(response);

            var dto =
                DeserializeBody<TResponse>(response);

            return dto;
        }

        protected TResponse SendAndDeserialize<TResponse>(string method, object request, string url)
        {
            var response = SendInternal(method, request, url);

            AssertResponse(response);

            var dto =
                DeserializeBody<TResponse>(response);

            return dto;
        }

        private static TResponse DeserializeBody<TResponse>(SimpleHttpResponse response)
        {
            return JsonDataContractDeserializer.Instance.DeserializeFromString<TResponse>(
                EncodeBody(response));
        }

        private static string EncodeBody(SimpleHttpResponse response)
        {
            return Encoding.UTF8.GetString(response.Body);
        }

        private void AssertResponse(SimpleHttpResponse response)
        {
            if (response.StatusCode >= 400)
                throw new WebServiceException()
                {
                    StatusCode = response.StatusCode,
                    ResponseBody = EncodeBody(response)
                };
        }

        protected void Send(string method, IReturnVoid request)
        {
            var url = request.ToUrl(method, Format);
            var response = SendInternal(method, request, url);

            AssertResponse(response);
        }
        
        private SimpleHttpResponse SendInternal(string method, object requestDto, string url)
        {
            if (method == null) throw new ArgumentNullException(nameof(method));

            method = method.ToUpper();
            var body = method == "GET" || requestDto == null 
                ? new byte[0] 
                : Encoding.UTF8.GetBytes(requestDto.ToJson());
            
            var response = Send(new SimpleHttpRequest
            {
                Method = method,
                Body = body,
                Url = $"{UrlPrefix}{url}",
                Headers = new List<HttpHeader>(),
            });
            return response;
        }

        public SimpleHttpResponse Send(string method, string url, object requestDto = null)
        {
            return SendInternal(method, requestDto, url);
        }

        public TResponse Get<TResponse>(IReturn<TResponse> request) => SendAndDeserialize("GET", request);

        public void Get(IReturnVoid request) => Send("GET", request);

        public TResponse Get<TResponse>(string relativeOrAbsoluteUrl) 
            => SendAndDeserialize("GET", new EmptyDto<TResponse>(), relativeOrAbsoluteUrl);
        public TResponse Delete<TResponse>(IReturn<TResponse> request) => SendAndDeserialize("DELETE", request);

        public void Delete(IReturnVoid request) => Send("DELETE", request);

        public TResponse Delete<TResponse>(string relativeOrAbsoluteUrl) 
            => SendAndDeserialize("DELETE", new EmptyDto<TResponse>(), relativeOrAbsoluteUrl);

        public TResponse Post<TResponse>(IReturn<TResponse> request) => SendAndDeserialize("POST", request);

        public void Post(IReturnVoid request) => Send("POST", request);

        public TResponse Post<TResponse>(string relativeOrAbsoluteUrl, object request) 
            => SendAndDeserialize<TResponse>("POST", request, relativeOrAbsoluteUrl);
        public TResponse Put<TResponse>(IReturn<TResponse> request) => SendAndDeserialize("PUT", request);

        public void Put(IReturnVoid request) => Send("PUT", request);

        public TResponse Put<TResponse>(string relativeOrAbsoluteUrl, object request) 
            => SendAndDeserialize<TResponse>("PUT", request, relativeOrAbsoluteUrl);
        public TResponse Patch<TResponse>(IReturn<TResponse> request) => SendAndDeserialize("PATCH", request);

        public void Patch(IReturnVoid request) => Send("PATCH", request);

        public TResponse Patch<TResponse>(string relativeOrAbsoluteUrl, object request) 
            => SendAndDeserialize<TResponse>("PATCH", request, relativeOrAbsoluteUrl);

        public void CustomMethod(string httpVerb, IReturnVoid request) => Send(httpVerb, request);

        public TResponse CustomMethod<TResponse>(string httpVerb, IReturn<TResponse> request) => SendAndDeserialize(httpVerb, request);

        public HttpWebResponse Head(IReturn request)
        {
            throw new System.NotImplementedException();
        }

        public HttpWebResponse Head(string relativeOrAbsoluteUrl)
        {
            throw new System.NotImplementedException();
        }

        public TResponse PostFile<TResponse>(string relativeOrAbsoluteUrl, FileInfo fileToUpload, string mimeType)
        {
            throw new System.NotImplementedException();
        }

        public SimpleHttpRestClient(Func<SimpleHttpRequest, SimpleHttpResponse> sendFunc, ILogger logger) 
            : base(sendFunc, logger)
        {
        }
    }

    class EmptyDto : IReturnVoid
    {

    }

    class EmptyDto<T> : IReturn<T>
    {

    }
}
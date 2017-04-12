using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ServiceStack.Common.Web;
using ServiceStack.LocalRequest.Contracts;
using ServiceStack.ServiceHost;

namespace ServiceStack.LocalRequest
{
    public class SimpleHttpResponseAdapter: IHttpResponse
    {
        private readonly SimpleHttpResponse _response;

        public SimpleHttpResponseAdapter(SimpleHttpResponse response)
        {
            _response = response;

            var c = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
            if(_response.Headers != null)
                foreach (var r in _response.Headers)
                    c.Add(r.Key, r.Value);
            Headers = c;

            _cookiesDict = new Dictionary<string, Cookie>(); 
            _cookies = new DictionaryCookiesAdapter(_cookiesDict);
        }

        public string ContentType
        {
            get { return Headers[HttpHeaders.ContentType]; }
            set { Headers[HttpHeaders.ContentType] = value; }
        }

        private DictionaryCookiesAdapter _cookies;

        public ICookies Cookies => _cookies;

        public int StatusCode
        {
            get { return _response.StatusCode; }
            set { _response.StatusCode = value; }
        }

        public string StatusDescription
        {
            get { return "nothing"; }
            set {  }
        }

        public void Close()
        {
            //Log.Debug("Closting response...");
            if (IsClosed)
                return;

            IsClosed = true;

            var cookies = _cookiesDict.Values;

            if (_outputStream.CanRead)
            {
                _outputStream.Position = 0;
                _response.Body = StreamUtil.ReadFully(_outputStream);
            }
            else
            {
                _response.Body = new byte[0];
            }

            _response.Headers = Headers.AllKeys.Select(key => new HttpHeader
            {
                Key = key,
                Value = Headers[key]
            }).ToList();

            _response.Headers.AddRange(cookies.Select(ToCookieHeader));

            if (StatusCode == 0)
                _response.StatusCode = (int)HttpStatusCode.OK;
        }

        private HttpHeader ToCookieHeader(Cookie cookie)
        {
            var secureStr = cookie.Secure ? "; secure" : "";
            var httpOnlyStr = cookie.HttpOnly ? "; httponly" : "";
            var pathStr = cookie.Path == null ? "" : $"; path={cookie.Path}";
            var expiresStr = cookie.Expires == DateTime.MaxValue
                ? ""
                : $"; expires={cookie.Expires.ToString("ddd, dd-MMM-yyyy HH:mm:ss")} GMT";

            return new HttpHeader
            {
                Key = HttpHeaders.SetCookie,
                Value = $"{cookie.Name}={cookie.Value}{expiresStr}{pathStr}{secureStr}{httpOnlyStr}"
            };
        }
        public void End()
        {
            Close();
        }

        public void Flush() { }

        public bool IsClosed { get; private set; }

        private readonly MemoryStream _outputStream = new MemoryStream();
        public Stream OutputStream => _outputStream;

        public void AddHeader(string name, string value) => Headers[name] = value;

        public object OriginalResponse => _response;

        private string _redirectUrl;
        private readonly Dictionary<string, Cookie> _cookiesDict;
        public void Redirect(string url) => _redirectUrl = url;

        public void SetContentLength(long contentLength) => Headers[HttpHeaders.ContentType] = contentLength.ToString();

        public void Write(string text)
        {
            var writer = new StreamWriter(_outputStream, Encoding.UTF8);
            writer.Write(text);
        }

        public NameValueCollection Headers { get; }
    }
}
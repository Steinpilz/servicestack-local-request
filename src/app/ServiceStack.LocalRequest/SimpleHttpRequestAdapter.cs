using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using ServiceStack.Common.Web;
using ServiceStack.LocalRequest.Contracts;
using ServiceStack.ServiceHost;
using ServiceStack.Text;
using ServiceStack.WebHost.Endpoints;
using ServiceStack.WebHost.Endpoints.Extensions;

namespace ServiceStack.LocalRequest
{
    public class SimpleHttpRequestAdapter : IHttpRequest
    {
        private readonly SimpleHttpRequest _request;

        public SimpleHttpRequestAdapter(SimpleHttpRequest request)
        {
            _request = request;
            var safeUrl = string.IsNullOrEmpty(request.Url) ? "/" : request.Url;
            var absoluteUrl = safeUrl.StartsWith("/") ? $"http://localhost{safeUrl}" : safeUrl;
            Uri = new Uri(absoluteUrl);

            OperationName = Uri.Segments.LastOrDefault();
            if (IsMultipartFormData)
            {
                TryToParseMultipartFormData();
            }
        }

        private void TryToParseMultipartFormData()
        {
            try
            {
                var parser = new MultipartFormDataParser();
                var result = parser.Parse(_request.Body);
                if(_formData == null) _formData = new NameValueCollection();

                _formData.Add(result.FormData);
                if(_files == null) _files = new List<IFile>();
                foreach (var httpFile in result.Files)
                {
                     _files.Add(httpFile);
                }
            }
            catch (Exception)
            {
                
            }
        }

        private Uri Uri { get; }

        public string AbsoluteUri => Uri.AbsoluteUri.TrimEnd('/');

        public string[] AcceptTypes => (Accept ?? string.Empty).Split(',');

        private string Accept => Headers[HttpHeaders.Accept];

        public string ApplicationFilePath => "";

        public string HttpMethod => _request.Method;

        public bool IsSecureConnection => false;

        public string ContentType
        {
            get
            {
                var type = Headers[HttpHeaders.ContentType];
                if (string.IsNullOrEmpty(type) || !type.Contains(";")) return Headers[HttpHeaders.ContentType];

                var originContentType = type.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries).First();
                if (originContentType.Contains(","))
                {
                    originContentType =
                        originContentType.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).First();
                }
                return originContentType.Trim();
            }
        } 

        private bool IsMultipartFormData
            => string.Equals(ContentType, "multipart/form-data", StringComparison.OrdinalIgnoreCase);

        public IDictionary<string, Cookie> Cookies
        {
            get
            {
                if (_cookies == null)
                {
                    _cookies = new Dictionary<string, Cookie>();

                    // todo: improve cookie parser
                    var cookieHeader = Headers["cookie"];
                    if (!string.IsNullOrEmpty(cookieHeader))
                    {
                        var pairs = cookieHeader.Split(new[] {"; "}, StringSplitOptions.RemoveEmptyEntries);
                        var cookies = pairs.Select(x => x.Split('=')).Where(a => a.Length == 2)
                            .Select(x => new Cookie
                            {
                                Name = x[0],
                                Value = x[1],
                                Path = "/",
                                Domain = UserHostAddress,
                            }).ToList();

                        foreach (var httpCookie in cookies)
                        {
                            Cookie cookie = null;

                            try
                            {
                                cookie = new Cookie(httpCookie.Name, httpCookie.Value, httpCookie.Path,
                                    httpCookie.Domain)
                                {
                                    HttpOnly = httpCookie.HttpOnly,
                                    Secure = httpCookie.Secure,
                                    Expires = httpCookie.Expires,
                                };
                            }
                            catch
                            {
                            }

                            if (cookie != null)
                                _cookies[httpCookie.Name] = cookie;
                        }
                    }
                }
                return _cookies;
            }
        }

        public bool IsLocal => true;

        public string OperationName { get; set; }

        public object OriginalRequest => _request;

        public string RawUrl => Uri.AbsolutePath;

        public string RemoteIp => null;

        public Stream InputStream => CreateFakeInputStream();

        private Stream CreateFakeInputStream()
        {
            if (_request.Body == null)
                return new MemoryStream();

            return StreamUtil.ToStream(_request.Body);
        }

        public string UserAgent => Headers["user-agent"];

        public string UserHostAddress => null;

        public string XForwardedFor => Headers[HttpHeaders.XForwardedFor];

        public string XRealIp => Headers[HttpHeaders.XRealIp];

        public T TryResolve<T>() => EndpointHost.AppHost.TryResolve<T>();

        private string pathInfo;

        public string PathInfo
        {
            get
            {
                if (this.pathInfo == null)
                {
                    var mode = EndpointHost.Config.ServiceStackHandlerFactoryPath;

                    var pos = RawUrl.IndexOf("?");
                    if (pos != -1)
                    {
                        var path = RawUrl.Substring(0, pos);
                        this.pathInfo = global::ServiceStack.WebHost.Endpoints.Extensions.HttpRequestExtensions
                            .GetPathInfo(
                                path,
                                mode,
                                mode ?? "");
                    }
                    else
                    {
                        this.pathInfo = RawUrl;
                    }

                    this.pathInfo = this.pathInfo.UrlDecode();
                    this.pathInfo = NormalizePathInfo(pathInfo, mode);
                }
                return this.pathInfo;
            }
        }

        private IList<IFile> _files;

        public IFile[] Files
        {
            get
            {
                if (_files == null)
                    _files = new IFile[0];
                return _files.ToArray();
            }

        }

        public long ContentLength
        {
            get
            {
                long ret = 0;
                if (Headers["content-length"] != null)
                    long.TryParse(Headers["content-length"], out ret);
                return ret;
            }
        }

        private NameValueCollection _formData;

        public NameValueCollection FormData
        {
            get
            {
                if(_formData == null) return new NameValueCollection();
                return _formData;
                //if (_formData == null)
                //{
                //    var formData = new NameValueCollection(StringComparer.OrdinalIgnoreCase);

                //    var contentType = ContentType?.Split(new[] { ";" }, 2, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? ContentType;
                //    if (FormContentTypes.Any(x => string.Equals(contentType, x, StringComparison.OrdinalIgnoreCase)))
                //    {
                //        var form = _request.ReadFormAsync().Result;
                //        foreach (var f in form)
                //            formData.Add(f.Key, string.Join("", f.Value));
                //    }

                //    _formData = formData;
                //}

                //return _formData;
            }
        }

        private NameValueCollection _headers;

        public NameValueCollection Headers
        {
            get
            {
                if (_headers != null) return _headers;

                var c = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
                if (_request.Headers != null)
                {
                    foreach (var r in _request.Headers)
                        c.Add(r.Key, r.Value);
                }
                _headers = c;

                return _headers;
            }
        }

        private Dictionary<string, object> items;

        public Dictionary<string, object> Items
        {
            get
            {
                if (items == null)
                    items = new Dictionary<string, object>();
                return items;
            }
        }

        private NameValueCollection _queryString;

        public NameValueCollection QueryString
        {
            get
            {
                if (_queryString == null)
                {
                    _queryString = ParseQueryString(Uri.Query);
                }

                return _queryString;
            }
        }

        private string responseContentType;
        private Dictionary<string, Cookie> _cookies;

        public string ResponseContentType
        {
            get { return responseContentType ?? (responseContentType = this.GetResponseContentType()); }
            set { this.responseContentType = value; }
        }

        public Uri UrlReferrer
        {
            get { return null; }
        }

        public bool UseBufferedStream
        {
            get { return true; }
            set { }
        }

        public string GetRawBody()
        {
            if (_request.Body == null)
                return "";
            return Encoding.UTF8.GetString(_request.Body);
        }

        private static string NormalizePathInfo(string pathInfo, string handlerPath)
        {
            if (handlerPath != null && pathInfo.TrimStart('/').StartsWith(
                handlerPath, StringComparison.InvariantCultureIgnoreCase))
            {
                return pathInfo.TrimStart('/').Substring(handlerPath.Length);
            }

            return pathInfo;
        }
        
        public static NameValueCollection ParseQueryString(string s)
        {
            var nvc = new NameValueCollection();

            // remove anything other than query string from url
            if (s.Contains("?"))
            {
                s = s.Substring(s.IndexOf('?') + 1);
            }

            foreach (string vp in Regex.Split(s, "&"))
            {
                var singlePair = Regex.Split(vp, "=");
                var value = singlePair.Length == 2 ? singlePair[1] : string.Empty;
                var key = singlePair[0];

                nvc.Add(HttpUtility.UrlDecode(key), HttpUtility.UrlDecode(value));
            }

            return nvc;
        }
    }
}
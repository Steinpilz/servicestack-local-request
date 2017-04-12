using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ServiceStack.LocalRequest.Contracts;
using Xunit;
using ServiceStack.ServiceHost;
using System.Text;

namespace ServiceStack.LocalRequest.Tests
{
    public class SimpleHttpRequestAdapterTests
    {
        [Fact]
        public void it_parses_query_string()
        {
            var request = new SimpleHttpRequest
            {
                Url = "/api/ping?format=json&test=name"
            };

            var adapter = new SimpleHttpRequestAdapter(request);

            var queryString = adapter.QueryString;

            Assert.Equal("json", queryString["format"]);
        }

        [Fact]
        public void it_returns_cookie_value()
        {
            var request = new SimpleHttpRequest
            {
                Headers = new List<HttpHeader>
                {
                    new HttpHeader
                    {
                        Key = "Cookie",
                        Value =
                            "_ga=GA1.4.1872600047.1460130975; NXSESSIONID=b8c3810f-1499-477c-9ccd-0f5e7478e81d; __utma=22858107.1872600047.1460130975.1461739745.1461739745.1; __utmc=22858107; __utmz=22858107.1461739745.1.1.utmcsr=(direct)|utmccn=(direct)|utmcmd=(none); API-SESSION-ID=243e918e6ad34926878d8ddcf0ee301b"
                    }
                }
            };

            var adapter = new SimpleHttpRequestAdapter(request);
            var apiSessionId = adapter.GetCookieValue("API-SESSION-ID");

            Assert.Equal("243e918e6ad34926878d8ddcf0ee301b", apiSessionId);
        }

        [Fact]
        public void it_should_parse_multipart_form_data()
        {
            var request = new SimpleHttpRequest
            {
                Headers = new List<HttpHeader>
                {
                    new HttpHeader {Key = "Content-Type", Value = "Multipart/form-data"}
                },
                Body = GetMultipartFormData()
            };

            var adapter = new SimpleHttpRequestAdapter(request);
            Assert.Equal(1, adapter.Files.Length);
        }

        [Fact]
        public void it_should_correct_parse_content_type_for_multipart_forms()
        {
            var request = new SimpleHttpRequest
            {
                Headers = new List<HttpHeader>
                {
                    new HttpHeader {Key = "Content-Type", Value = "multipart/form-data, multipart/form-data; boundary=---------------------------7e0fd5d10ae"} //this is from ie
                },
                Body = GetMultipartFormData()
            };

            var adapter = new SimpleHttpRequestAdapter(request);
            
            var request2 = new SimpleHttpRequest
            {
                Headers = new List<HttpHeader>
                {
                    new HttpHeader {Key = "Content-Type", Value = " multipart/form-data; boundary=---------------------------7e0fd5d10ae"} //this is from ie too
                },
                Body = GetMultipartFormData()
            };

            var adapter2 = new SimpleHttpRequestAdapter(request2);
            Assert.Equal("multipart/form-data", adapter.ContentType);
            Assert.Equal("multipart/form-data", adapter2.ContentType);
        }

        [Fact]
        public void it_should_parse_correct_file_name_for_multipart_forms()
        {
            var request = new SimpleHttpRequest
            {
                Headers = new List<HttpHeader>
                {
                    new HttpHeader {Key = "Content-Type", Value = "multipart/form-data"}
                },
                Body = GetMultipartFormData(@"c:\ssts\text.text")
            };


            var adapter = new SimpleHttpRequestAdapter(request);

            var request2 = new SimpleHttpRequest
            {
                Headers = new List<HttpHeader>
                {
                    new HttpHeader {Key = "Content-Type", Value = "multipart/form-data"}
                },
                Body = GetMultipartFormData(@"/root/user/text.text")
            };


            var adapter2 = new SimpleHttpRequestAdapter(request2);
            Assert.Equal(1, adapter.Files.Length);
            Assert.Equal("text.text", adapter.Files[0].FileName);
            Assert.Equal(1, adapter2.Files.Length);
            Assert.Equal("text.text", adapter2.Files[0].FileName);
        }

        private static byte[] GetMultipartFormData(string filename= "test.txt")
        {
            string boundary = $"----------{Guid.NewGuid().ToString("N")}";
            Stream formDataStream = new System.IO.MemoryStream();
            var encoding = Encoding.UTF8;

            const string key = "count";
            const int value = 10;

            string postData = $"--{boundary}\r\nContent-Disposition: form-data; name=\"{key}\"\r\n\r\n{value}";
            formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));

            formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));
            const string contentType = "application/octet-stream";
            const string fileKey = "file";
            var header =
                $"--{boundary}\r\nContent-Disposition: form-data; name=\"{fileKey}\"; filename=\"{filename}\"\r\nContent-Type: {contentType}\r\n\r\n";
            formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));
            //var content = File.ReadAllBytes(@"C:\temp\test\SanFrancisco-CoastCorrected.bmp");
            //formDataStream.Write(content, 0, content.Length);
            formDataStream.Write(encoding.GetBytes("HELLO WORLD!"), 0, encoding.GetByteCount("HELLO WORLD!"));
    

            var footer = $"\r\n--{boundary}--\r\n";
            formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

            // Dump the Stream into a byte[]
            formDataStream.Position = 0;
            var formData = new byte[formDataStream.Length];
            formDataStream.Read(formData, 0, formData.Length);
            formDataStream.Close();
            return formData;
        }
    }
}
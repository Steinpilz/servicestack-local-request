using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ServiceStack.ServiceHost;
using System.Linq;

namespace ServiceStack.LocalRequest
{
    public class MultipartFormDataParser
    {
        private const string NewLine = "\r\n";

        public ParseResult Parse(IHttpRequest req)
        {
            req.InputStream.Position = 0L;
            var body = StreamUtil.ReadFully(req.InputStream);
            req.InputStream.Position = 0L;
            return Parse(body);
        }

        public ParseResult Parse(byte[] body)
        {
            var content = Encoding.UTF8.GetString(body);
            // The first line should contain the delimiter
            var delimiterEndIndex = content.IndexOf(NewLine);

            if (delimiterEndIndex <= -1) return new ParseResult();
            var delimiter = content.Substring(0, delimiterEndIndex);
            return ParseBinary(delimiter, body);
        }

        private static ParseResult ParseBinary(string delimiterString, byte[] streamBytes)
        {
            var files = new List<HttpFile>();
            var formData = new NameValueCollection();
            var delimiterWithNewLineBytes = Encoding.UTF8.GetBytes(delimiterString + "\r\n");
            // the request ends DELIMITER--\r\n
            var delimiterEndBytes = Encoding.UTF8.GetBytes("\r\n" + delimiterString + "--\r\n");
            var lengthDifferenceWithEndBytes = (delimiterString + "--\r\n").Length;

            // seperate by delimiter + newline
            // ByteArraySplit code found at http://stackoverflow.com/a/9755250/4244411
            var separatedStream = Separate(streamBytes, delimiterWithNewLineBytes);
            for (var i = 0; i < separatedStream.Length; i++)
            {
                // parse out whether this is a parameter or a file
                // get the first line of the byte[] as a string
                var thisPieceAsString = Encoding.UTF8.GetString(separatedStream[i]);

                if (string.IsNullOrWhiteSpace(thisPieceAsString))
                {
                    continue;
                }

                var firstLine = thisPieceAsString.Substring(0, thisPieceAsString.IndexOf("\r\n"));

                // Check the item to see what it is
                var regQuery = new Regex(@"(?<=name\=\"")(.*?)(?=\"")");
                var regMatch = regQuery.Match(firstLine);
                var propertyType = regMatch.Value.Trim();
                var filenameQuery = new Regex(@"(?<=filename\=\"")(.*?)(?=\"")");
                var filenameMatch = filenameQuery.Match(firstLine);

                // get the index of the start of the content and the end of the content
                var indexOfStartOfContent = thisPieceAsString.IndexOf("\r\n\r\n") + "\r\n\r\n".Length;

                if (filenameMatch.Success && regMatch.Success)
                {
                    var fileName = filenameMatch.Value.Trim();
                    var contentTypeRegex = new Regex(@"(?<=Content\-Type:)(.*?)(?=\r\n\r\n)");
                    var contentTypeMatch = contentTypeRegex.Match(thisPieceAsString);
                    var contentType = string.Empty;
                    if (contentTypeMatch.Success)
                    {
                        contentType = contentTypeMatch.Value.ToString();
                    }

                    // get the content byte[]
                    // if this is the last piece, chop off the final delimiter
                    var lengthToRemove = (i == separatedStream.Length - 1) ? delimiterEndBytes.Length : 0;
                    var contentByteArrayStartIndex =
                        Encoding.UTF8.GetBytes(thisPieceAsString.Substring(0, indexOfStartOfContent)).Length;
                    var fileData = new byte[separatedStream[i].Length - contentByteArrayStartIndex - lengthToRemove];
                    Buffer.BlockCopy(separatedStream[i], contentByteArrayStartIndex, fileData, 0, separatedStream[i].Length - contentByteArrayStartIndex - lengthToRemove);
                    fileName = Path.GetFileName(fileName.Trim());
                    files.Add(new HttpFile()
                    {
                        ContentType = contentType?.Trim(),
                        ContentLength = fileData.Length,
                        FileName = fileName,
                        InputStream = new MemoryStream(fileData)
                    });
                }
                else
                {
                    // this is a parameter!
                    // if this is the last piece, chop off the final delimiter
                    var lengthToRemove = (i == separatedStream.Length - 1) ? lengthDifferenceWithEndBytes : 0;
                    var value = thisPieceAsString.Substring(indexOfStartOfContent,
                        thisPieceAsString.Length - "\r\n".Length - indexOfStartOfContent - lengthToRemove);
                    formData.Add(propertyType?.Trim(), value?.Trim());
                }


            }

            return new ParseResult()
            {
                Files = files,
                FormData = formData
            };
        }


        private static byte[][] Separate(byte[] source, IReadOnlyCollection<byte> separator)
        {
            var parts = new List<byte[]>();
            var index = 0;
            byte[] part;
            for (var i = 0; i < source.Length; ++i)
            {
                if (!Equals(source, separator, i)) continue;
                part = new byte[i - index];
                Array.Copy(source, index, part, 0, part.Length);
                parts.Add(part);
                index = i + separator.Count;
                i += separator.Count - 1;
            }
            part = new byte[source.Length - index];
            Array.Copy(source, index, part, 0, part.Length);
            parts.Add(part);
            return parts.ToArray();
        }

        private static bool Equals(IReadOnlyList<byte> source, IEnumerable<byte> separator, int index)
        {
            return !separator.Where((t, i) => index + i >= source.Count || source[index + i] != t).Any();
        }

        
    }

    public class ParseResult
    {
        public ParseResult()
        {
            Files = new List<HttpFile>();
            FormData = new NameValueCollection();
        }
        public IEnumerable<HttpFile> Files { get; set; }
        public NameValueCollection FormData { get; set; }

    }
}
using ServiceStack.LocalRequest.Contracts;
using System.Linq;
using System.Text;

namespace ServiceStack.LocalRequest.Debug
{
    class ResponseDumper : Dumper
    {
        private readonly SimpleHttpResponse response;

        public ResponseDumper(SimpleHttpResponse response)
        {
            this.response = response;
        }

        protected override void DumpImpl()
        {
            GeneralInfo();
            Headers();
            Body();
        }

        void Body()
        {
            // try get utf 8 string
            if ((response.Body?.Length ?? 0) == 0)
            {
                AppendLine("Body: NULL");
                return;
            }

            AppendLine("Body (decoded UTF-8):");

            var str = Encoding.UTF8.GetString(response.Body);

            AppendLine("---BODY-START---");
            AppendLine("");

            AppendLine(str);

            AppendLine("");
            AppendLine("---BODY-END---");
        }

        void Headers()
        {
            AppendLine($"Headers ({response.Headers?.Count ?? 0}):");
            Tab(() =>
            {
                if (response.Headers != null)
                    foreach (var header in response.Headers)
                    {
                        AppendLine($"[{header.Key}] =");
                        Tab(() =>
                        {
                            AppendLine($"[{header.Value}]");
                        });
                    }
            });
        }

        void GeneralInfo()
        {
            AppendLine($"Status Code: {response.StatusCode}");
        }
    }
}

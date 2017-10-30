using ServiceStack.LocalRequest.Contracts;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStack.LocalRequest.Debug
{

    class RequestDumper : Dumper
    {
        private readonly SimpleHttpRequest request;
        
        public RequestDumper(SimpleHttpRequest request)
        {
            this.request = request;
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
            if ( (request.Body?.Length ?? 0) == 0)
            {
                AppendLine("Body: NULL");
                return;
            }

            AppendLine("Body (decoded UTF-8):");

            var str = Encoding.UTF8.GetString(request.Body);

            AppendLine("---BODY-START---");
            AppendLine("");

            AppendLine(str);

            AppendLine("");
            AppendLine("---BODY-END---");
        }

        void Headers()
        {
            AppendLine($"Headers ({request.Headers?.Count ?? 0}):");
            Tab(() =>
            {
                if(request.Headers != null)
                    foreach(var header in request.Headers)
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
            AppendLine($"{request.Method} {request.Url}");
        }
    }
}

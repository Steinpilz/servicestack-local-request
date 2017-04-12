using System.Collections.Generic;
using System.Runtime.Serialization;
using ProtoBuf;

namespace ServiceStack.LocalRequest.Contracts
{
    [DataContract]
    [ProtoContract]
    public class SimpleHttpResponse
    {
        [ProtoMember(1)]
        [DataMember(Order=1)]
        public int StatusCode { get; set; }

        [ProtoMember(2)]
        [DataMember(Order=2)]
        public List<HttpHeader> Headers { get; set; }

        [ProtoMember(3)]
        [DataMember(Order=3)]
        public byte[] Body { get; set; }

        [ProtoMember(4, DynamicType = true)]
        [DataMember(Order=4)]
        public object Dto { get; set; }
    }
}
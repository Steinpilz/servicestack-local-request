using System.Collections.Generic;
using System.Runtime.Serialization;
using ProtoBuf;

namespace ServiceStack.LocalRequest.Contracts
{
    [DataContract]
    [ProtoContract]
    public class SimpleHttpRequest
    {
        [ProtoMember(1)]
        [DataMember(Order=1)]
        public string Method { get; set; }

        [ProtoMember(2)]
        [DataMember(Order=2)]
        public string Url { get; set; }

        [ProtoMember(3)]
        [DataMember(Order=3)]
        public List<HttpHeader> Headers { get; set; }

        [ProtoMember(4)]
        [DataMember(Order=4)]
        public byte[] Body { get; set; }

        [ProtoMember(5, DynamicType = true)]
        [DataMember(Order=5)]
        public object Dto { get; set; }
    }
}
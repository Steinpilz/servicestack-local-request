using System.Runtime.Serialization;
using ProtoBuf;

namespace ServiceStack.LocalRequest.Contracts
{
    [ProtoContract]
    [DataContract]
    public class HttpHeader
    {
        [ProtoMember(1)]
        [DataMember(Order=1)]
        public string Key { get; set; }

        [ProtoMember(1)]
        [DataMember(Order=2)]
        public string Value { get; set; }
    }
}
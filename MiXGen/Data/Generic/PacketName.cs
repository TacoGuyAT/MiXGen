using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MiXGen.Data.Generic {
    public struct PacketName {
        public string Value {
            get {
                switch(Selected) {
                    case NameType.MiNET:
                        return MiNET;
                    case NameType.gophertunnel_original:
                        return gophertunnelOriginal;
                    default:
                        return gophertunnel;
                }
            }
        }
        public string MiNET;
        public string gophertunnel;
        public string gophertunnelOriginal;
        public NameType Selected;
    }
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NameType {
        MiNET,
        gophertunnel,
        gophertunnel_original
    }
}

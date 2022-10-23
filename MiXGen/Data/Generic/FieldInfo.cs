using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiXGen.Data.Generic {
    public struct FieldInfo {
        public bool IsOptional;
        public string Name;
        public FieldType Type;
        public Endianess Endianess;
    }
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Endianess {
        BigEndian,
        LittleEndian
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiXGen.Data.Generic {
    public class PacketInfo {
        public PacketInfo(ulong ID) {
            this.Name = new PacketName {
                MiNET = $"MCPE_UNKNOWN_PACKET_{ID.ToString("X2")}",
                gophertunnel = $"MCPE_UNKNOWN_PACKET_{ID.ToString("X2")}",
                gophertunnelOriginal = $"IDUnknownPacket{ID.ToString("X2")}",
                Selected = NameType.gophertunnel
            };
        }
        public ulong ID;
        public PacketName Name;
        public string? Namespace;
        public List<FieldInfo> Fields = new();  // Must be in the same order!
        public List<EnumInfo> Enums = new();
        public bool IsOnline;
        public bool IsClient;
        public bool IsServer;
    }
}

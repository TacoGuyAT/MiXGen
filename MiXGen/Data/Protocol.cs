using MiXGen.Data.Generic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace MiXGen.Data {
    public static class Protocol {

		#region PATHS
		public const string MiNET_XML = @"/src/MiNET/MiNET/Net/MCPE Protocol.xml";

		public const string GOPHER_IDS = @"/minecraft/protocol/packet/id.go";
		public const string GOPHER_PACKETS = @"/minecraft/protocol/packet";
		public const string GOPHER_INFO = @"/minecraft/protocol/info.go";
		#endregion

		#region gophertunnel Types
		/*
			Uint16(x *uint16)
			Int16(x *int16)
			Uint32(x *uint32)
			Int32(x *int32)
			BEInt32(x *int32)
			Uint64(x *uint64)
			Int64(x *int64)
			Float32(x *float32)
			Uint8(x *uint8)
			Int8(x *int8)
			Bool(x *bool)
			Varint64(x *int64)
			Varuint64(x *uint64)
			Varint32(x *int32)
			Varuint32(x *uint32)
			String(x *string)
			StringUTF(x *string)
			ByteSlice(x *[]byte)
			Vec3(x *mgl32.Vec3)
			Vec2(x *mgl32.Vec2)
			BlockPos(x *BlockPos)
			UBlockPos(x *BlockPos)
			ChunkPos(x *ChunkPos)
			SubChunkPos(x *SubChunkPos)
			ByteFloat(x *float32)
			Bytes(p *[]byte)
			NBT(m *map[string]any, encoding nbt.Encoding)
			NBTList(m *[]any, encoding nbt.Encoding)
			UUID(x *uuid.UUID)
			RGBA(x *color.RGBA)
			VarRGBA(x *color.RGBA)
			EntityMetadata(x *map[uint32]any)
			Item(x *ItemStack)
			ItemInstance(i *ItemInstance)
			ItemDescriptorCount(i *ItemDescriptorCount)
			MaterialReducer(x *MaterialReducer)
			GameRule(x *GameRule)

			UnknownEnumOption(value any, enum string)
			InvalidValue(value any, forField, reason string)
         */
		#endregion

		public static Dictionary<string, string> GOPHER_MiNET_TYPE = new Dictionary<string, string> {
			{ "Bool", "bool" },
		};
		public static Dictionary<ulong, PacketInfo> PacketsCurrent = new();
		public static Dictionary<ulong, PacketInfo> PacketsNew = new();
		public static XmlDocument Xml = new XmlDocument();
		public static ProtocolInfo MiNETInfo = new ProtocolInfo { GameVersion = "0.0.0", ProtocolVersion = 0 };
		public static ProtocolInfo gophertunnelInfo = new ProtocolInfo { GameVersion = "0.0.0", ProtocolVersion = 0 };

		public static void LoadXML(string path, Dictionary<ulong, PacketInfo> packets) {
			Xml.Load(path + MiNET_XML);
			MiNETInfo.ProtocolVersion = ulong.Parse(Xml.SelectNodes("/protocol")![0]!.Attributes!["protocolVersion"]!.Value);
			MiNETInfo.GameVersion = Xml.SelectNodes("/protocol")![0]!.Attributes!["gameVersion"]!.Value;
			var packetsNode = Xml.SelectNodes("//pdu[not(@namespace='raknet') and not(@namespace='ftl')]")!;
			foreach(XmlNode packetNode in packetsNode) {
				var id = ulong.Parse(packetNode.Attributes!["id"]!.Value.Substring(2), NumberStyles.HexNumber);
				if(!packets.ContainsKey(id))
                    packets.Add(id, new PacketInfo(id));

				var packet = packets[id];

				packet.Name.MiNET = packetNode.Attributes!["name"]!.Value;
				packet.Name.Selected = NameType.MiNET;

				if(packetNode.Attributes!["online"] != null)
					packet.IsOnline = packetNode.Attributes!["online"]!.Value == "true";
				if(packetNode.Attributes!["client"] != null)
					packet.IsClient = packetNode.Attributes!["client"]!.Value == "true";
				if(packetNode.Attributes!["server"] != null)
					packet.IsServer = packetNode.Attributes!["server"]!.Value == "true";
				if(packetNode.Attributes!["namespace"] != null)
					packet.Namespace = packetNode.Attributes!["namespace"]!.Value;

                packets[id] = packet;

				var fieldsNode = packetNode.SelectNodes("field");
				
				if(fieldsNode != null)
					foreach(XmlNode fieldNode in fieldsNode) {
						var type = fieldNode.Attributes!["type"]!.Value;
						var fields = packets[id].Fields;

						if(fields.Any(f => f.Name == fieldNode.Attributes["name"]!.Value))
							packet.Fields.Remove(fields.Find(f => f.Name == fieldNode.Attributes["name"]!.Value));

						var field = new FieldInfo();

						field.Name = fieldNode.Attributes!["name"]!.Value;
						field.Type = type.EndsWith("[]") ? new FieldType { IsArray = true, Name = type.Substring(0, type.Length - 2) } : new FieldType { IsArray = false, Name = type };

						if(fieldNode.Attributes!["endianess"] != null)
							field.Endianess = fieldNode.Attributes!["endianess"]!.Value == "BE" ? Endianess.BigEndian : Endianess.LittleEndian;
						if(fieldNode.Attributes!["optional"] != null)
							field.IsOptional = fieldNode.Attributes!["optional"]!.Value == "true";

						fields.Add(field);
					}
			}
		}
		public static string ToMiNETPacketName(string gopherID, ulong id = 0) {
			if(gopherID == "\t_")
				return $"MCPE_UNKNOWN_PACKET_ID_{id.ToString("X2")}";
			gopherID = Regex.Replace(gopherID, @"(\\[a-zA-Z0-9])|[^a-zA-Z0-9]", "");
			if(gopherID.StartsWith("ID"))
				gopherID = gopherID.Substring(2);
			return "MCPE" + String.Join('_', Regex.Split(gopherID, @"(?=[A-Z][a-z])|(?=[A-Z])(?<![A-Z])")).ToUpper();
		}
		public static string ToMiNETFieldName(string gopherField, ulong id = 0) {
			gopherField = Regex.Replace(gopherField, @"(\\[a-zA-Z0-9])|[^a-zA-Z0-9]", "");
			return String.Join(' ', Regex.Split(gopherField, @"(?=[A-Z][a-z])|(?=[A-Z])(?<![A-Z])")).Substring(1);
		}
		public static string ToGopherPacketSourceFileName(string gopherID, ulong id = 0) {
			if(gopherID == "\t_")
				return $"unknown.go";
			gopherID = Regex.Replace(gopherID, @"(\\[a-zA-Z0-9])|[^a-zA-Z0-9]", "");
			if(gopherID.StartsWith("ID"))
				gopherID = gopherID.Substring(2);
			return "/" + String.Join('_', Regex.Split(gopherID, @"(?=[A-Z][a-z])|(?=[A-Z])(?<![A-Z])")).ToLower().Substring(1) + ".go";
		}
		public static void GenerateProtocolFromSource(string path) {
			var info = File.ReadAllText(path + GOPHER_INFO);
			gophertunnelInfo.GameVersion = info.Split("CurrentVersion = \"")[1].Split("\"\n")[0];
			gophertunnelInfo.ProtocolVersion = ulong.Parse(info.Split("CurrentProtocol = ")[1].Split('\n')[0]);
			GenerateIDsFromSource(path);
			for(ulong i = 1; i < (ulong)PacketsNew.Count; i++)
				PacketsNew[i] = GenerateFieldsFromSource(PacketsNew[i], path);
		}
		public static PacketInfo GenerateFieldsFromSource(PacketInfo packet, string path) {
			try {
				var source = File.ReadAllText(path + GOPHER_PACKETS + ToGopherPacketSourceFileName(packet.Name.gophertunnelOriginal));
				var names = source.Split(" struct {")[1].Split("}")[0].Replace("\t", "").Split('\n').Where(f => !f.StartsWith("//")).Select(f => f.Split(' ')).Where(s => s[0] != "");
				var fields = source
					.Split("Marshal(")[1]
					.Split(") {")[1]
					.Split('}')[0]
					.Replace("\t", "")
					.Split('\n')
					.Where(s => s != "")
					.Select(f => {
						var o = f.Replace("w.", "")
							.Replace('(', ' ')
							.Replace(")", "")
							.Split(' ');
						o[1] = o[1].Split('.')[1];
						return o;
					});

				Debug.WriteLine(JsonConvert.SerializeObject(fields));

				if(fields.Count() != names.Count()) {
					Debug.WriteLine($"Failed to convert packet \"{packet.Name.gophertunnelOriginal}\" (0x{packet.ID.ToString("X2")})");
					return packet;
				}

				packet.Fields.Clear();

				foreach(var fieldInfo in fields) {
					var field = new FieldInfo();

					field.Name = ToMiNETFieldName(fieldInfo[1]);
					field.Type = new FieldType { IsArray = false, Name = fieldInfo[0] };
					field.Endianess = Endianess.LittleEndian;
					if(fieldInfo[0].StartsWith("BE")) {
						field.Endianess = Endianess.BigEndian;
						field.Type.Name = fieldInfo[0].Substring(2);
                    }

					// gophertunnel uses if/else, will need a lot more complicated thing
					//field.IsOptional = ;

					packet.Fields.Add(field);
				}
			} catch(Exception e) {
				Debug.WriteLine(e.StackTrace);
			}
			return packet;
		}
		public static void GenerateIDsFromSource(string path) {
			var lines = File.ReadAllLines(path + GOPHER_IDS).ToList();
			var line = lines.First();
			while(!line.Contains(" = iota + 1")) {
				lines.RemoveAt(0);
				line = lines.First();
            }
			lines[0] = lines[0].Replace(" = iota + 1", "");
			ulong id = 1;
			foreach(string s in lines) {
				if(s.Contains(")"))
					break;

				if(!PacketsNew.ContainsKey(id))
					PacketsNew.Add(id, new PacketInfo(id));
				var packet = PacketsNew[id];

				if(packet.Name.MiNET.Contains("UNKNOWN_PACKET"))
					packet.Name.Selected = NameType.gophertunnel;
				packet.Name.gophertunnelOriginal = s.Replace("\t", "");
				packet.Name.gophertunnel = ToMiNETPacketName(s, id);

				PacketsNew[id] = packet;

				id++;
			}
		}
	}
}

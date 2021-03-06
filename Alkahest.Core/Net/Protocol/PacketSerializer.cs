using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using Alkahest.Core.IO;
using Alkahest.Core.Net.Protocol.OpCodes;

namespace Alkahest.Core.Net.Protocol
{
    public abstract class PacketSerializer
    {
        internal const BindingFlags FieldFlags =
            BindingFlags.DeclaredOnly |
            BindingFlags.Instance |
            BindingFlags.Public;

        public MessageTables Messages { get; }

        readonly ConcurrentDictionary<Type, IReadOnlyList<PacketFieldInfo>> _info =
            new ConcurrentDictionary<Type, IReadOnlyList<PacketFieldInfo>>();

        readonly IReadOnlyDictionary<ushort, Func<Packet>> _packetCreators;

        protected PacketSerializer(MessageTables messages)
        {
            Messages = messages ??
                throw new ArgumentNullException(nameof(messages));

            var creators = new Dictionary<ushort, Func<Packet>>();

            using (var container = new CompositionContainer(
                new AssemblyCatalog(Assembly.GetExecutingAssembly()), true))
                foreach (var lazy in container.GetExports<Func<Packet>,
                    IPacketMetadata>(PacketAttribute.ThisContractName))
                    if (messages.Game.NameToOpCode.ContainsKey(lazy.Metadata.OpCode))
                        creators.Add(messages.Game.NameToOpCode[lazy.Metadata.OpCode],
                            lazy.Value);

            _packetCreators = creators;
        }

        public bool IsKnown(ushort opCode)
        {
            return _packetCreators.ContainsKey(opCode);
        }

        public Type GetType(ushort opCode)
        {
            _packetCreators.TryGetValue(opCode, out var creator);

            return creator?.Method.DeclaringType;
        }

        public Packet Create(ushort opCode)
        {
            _packetCreators.TryGetValue(opCode, out var creator);

            return creator?.Invoke();
        }

        protected abstract PacketFieldInfo CreateFieldInfo(
            PropertyInfo property, PacketFieldAttribute attribute);

        protected IReadOnlyList<T> GetPacketFields<T>(Type type)
            where T : PacketFieldInfo
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return _info.GetOrAdd(type, t =>
            {
                return (from prop in t.GetProperties(FieldFlags)
                        let attr = prop.GetCustomAttribute<PacketFieldAttribute>()
                        where attr != null
                        where attr.Regions.Length == 0 ||
                            attr.Regions.Contains(Messages.Region)
                        orderby prop.MetadataToken
                        select CreateFieldInfo(prop, attr)).ToArray();
            }).Cast<T>().ToArray();
        }

        protected abstract void OnSerialize(TeraBinaryWriter writer,
            Packet packet);

        public byte[] Serialize(Packet packet)
        {
            if (packet == null)
                throw new ArgumentNullException(nameof(packet));

            packet.OnSerialize(this);

            using (var writer = new TeraBinaryWriter())
            {
                OnSerialize(writer, packet);

                return writer.ToArray();
            }
        }

        protected abstract void OnDeserialize(TeraBinaryReader reader,
            Packet packet);

        public void Deserialize(byte[] payload, Packet packet)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            if (packet == null)
                throw new ArgumentNullException(nameof(packet));

            using (var reader = new TeraBinaryReader(payload))
                OnDeserialize(reader, packet);

            packet.OnDeserialize(this);
        }
    }
}

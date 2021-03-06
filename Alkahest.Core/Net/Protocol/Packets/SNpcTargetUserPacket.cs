using Alkahest.Core.Game;

namespace Alkahest.Core.Net.Protocol.Packets
{
    public sealed class SNpcTargetUserPacket : Packet
    {
        const string Name = "S_NPC_TARGET_USER";

        public override string OpCode => Name;

        [Packet(Name)]
        internal static Packet Create()
        {
            return new SNpcTargetUserPacket();
        }

        [PacketField]
        public EntityId Source { get; set; }

        [PacketField]
        public byte Unknown1 { get; set; }
    }
}

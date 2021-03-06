using Alkahest.Core.Game;

namespace Alkahest.Core.Net.Protocol.Packets
{
    public sealed class SStartCoolTimeSkillPacket : Packet
    {
        const string Name = "S_START_COOLTIME_SKILL";

        public override string OpCode => Name;

        [Packet(Name)]
        internal static Packet Create()
        {
            return new SStartCoolTimeSkillPacket();
        }

        [PacketField]
        public SkillId Skill { get; set; }

        [PacketField]
        public uint Cooldown { get; set; }
    }
}

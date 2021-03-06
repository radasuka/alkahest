namespace Alkahest.Core.Net.Protocol.Packets
{
    public sealed class CAvailableEventMatchingListPacket : Packet
    {
        const string Name = "C_AVAILABLE_EVENT_MATCHING_LIST";

        public override string OpCode => Name;

        [Packet(Name)]
        internal static Packet Create()
        {
            return new CAvailableEventMatchingListPacket();
        }

        [PacketField]
        public byte Unknown1 { get; set; }
    }
}

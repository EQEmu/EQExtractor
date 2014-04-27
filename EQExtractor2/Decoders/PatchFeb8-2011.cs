using EQExtractor2.Domain;

namespace EQExtractor2.Decoders
{
    class PatchFeb082011Decoder : PatchDec072010Decoder
    {
        public PatchFeb082011Decoder()
        {
            Version = "EQ Client Build Date February 8 2011.";

            PatchConfFileName = "patch_Feb8-2011.conf";

            ExpectedPPLength = 28176;

            PPZoneIDOffset = 20844;
        }

        override public IdentificationStatus Identify(int opCode, int size, PacketDirection direction)
        {
            if ((opCode == OpManager.OpCodeNameToNumber("OP_ZoneEntry")) && (direction == PacketDirection.ClientToServer))
            {
                IDStatus = IdentificationStatus.Tentative;
                return IdentificationStatus.Tentative;
            }

            if (IDStatus != IdentificationStatus.Tentative)
                return IdentificationStatus.No;

            if ((opCode == OpManager.OpCodeNameToNumber("OP_PlayerProfile")) && (direction == PacketDirection.ServerToClient) &&
                (size == ExpectedPPLength))
                return IdentificationStatus.Yes;

            return IdentificationStatus.No;
        }

        override public Item DecodeItemPacket(byte[] packetBuffer)
        {
            ByteStream Buffer = new ByteStream(packetBuffer);

            Item NewItem = new Item();

            Buffer.SetPosition(30);
            NewItem.MerchantSlot = Buffer.ReadByte();            // 13
            NewItem.Price = Buffer.ReadUInt32();                 // 14
            Buffer.SkipBytes(5);
            NewItem.Quantity = Buffer.ReadInt32();               // 23
            Buffer.SetPosition(97);
            NewItem.Name = Buffer.ReadString(true);
            NewItem.Lore = Buffer.ReadString(true);
            NewItem.IDFile = Buffer.ReadString(true);
            Buffer.SkipBytes(1);
            NewItem.ID = Buffer.ReadUInt32();

            return NewItem;
        }
    }
}
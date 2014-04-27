using System;
using System.IO;
using EQExtractor2.Domain;

namespace EQExtractor2.Decoders
{
    class PatchDec072010Decoder : PatchOct202010Decoder
    {
        public PatchDec072010Decoder()
        {
            Version = "EQ Client Build Date December 7 2010.";

            PatchConfFileName = "patch_Dec7-2010.conf";

        }
        override public IdentificationStatus Identify(int opCode, int size, PacketDirection direction)
        {
            if ((opCode == OpManager.OpCodeNameToNumber("OP_ZoneEntry")) && (direction == PacketDirection.ClientToServer))
                return IdentificationStatus.Tentative;

            if ((opCode == OpManager.OpCodeNameToNumber("OP_SendAATable")) && (direction == PacketDirection.ServerToClient) &&
                (size == 120))
                return IdentificationStatus.Yes;

            return IdentificationStatus.No;
        }

        public override void RegisterExplorers()
        {
            //OpManager.RegisterExplorer("OP_ClientUpdate", ExploreClientUpdate);
        }

        public override void ExploreClientUpdate(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            UInt16 SpawnID = buffer.ReadUInt16();
            buffer.SkipBytes(6);
            float x = buffer.ReadSingle();
            float y = buffer.ReadSingle();
            buffer.SkipBytes(12);
            float z = buffer.ReadSingle();

            buffer.SkipBytes(4);
            UInt32 Temp = buffer.ReadUInt32();
            Temp = Temp & 0x3FFFFF;
            Temp = Temp >> 10;
            float heading = Utils.EQ19ToFloat((Int32)(Temp));

            outputStream.WriteLine("Loc: {0}, {1}, {2}  Heading: {3}", x, y, z, heading);

            outputStream.WriteLine("");
        }
    }
}
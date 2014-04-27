using System;
using System.Collections.Generic;
using System.IO;
using EQExtractor2.Domain;

namespace EQExtractor2.Decoders
{
    class PatchMar152012Decoder : PatchNov172011Decoder
    {
        public PatchMar152012Decoder()
        {
            Version = "EQ Client Build Date March 15 2012.";

            ExpectedPPLength = 29688;

            PPZoneIDOffset = 22356;

            PatchConfFileName = "patch_Mar15-2012.conf";
        }

        public override void RegisterExplorers()
        {
            //OpManager.RegisterExplorer("OP_ZoneEntry", ExploreZoneEntry);
        }

        public override void ExploreZoneEntry(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            if (direction != PacketDirection.ServerToClient)
                return;

            string SpawnName = buffer.ReadString(true);

            UInt32 SpawnID = buffer.ReadUInt32();

            byte Level = buffer.ReadByte();

            float UnkSize = buffer.ReadSingle();

            byte IsNPC = buffer.ReadByte();

            UInt32 Bitfield = buffer.ReadUInt32();
            /*
            NewSpawn.Showname = (Bitfield >> 28) & 1;
            NewSpawn.TargetableWithHotkey = (Bitfield >> 27) & 1;
            NewSpawn.Targetable = (Bitfield >> 26) & 1;

            NewSpawn.ShowHelm = (Bitfield >> 24) & 1;
            NewSpawn.Gender = (Bitfield >> 20) & 3;

            NewSpawn.Padding5 = (Bitfield >> 4) & 1;
            NewSpawn.Padding7 = (Bitfield >> 6) & 2047;
            NewSpawn.Padding26 = (Bitfield >> 25) & 1;
            */
            Byte OtherData = buffer.ReadByte();

            buffer.SkipBytes(8);    // Skip 8 unknown bytes

            //NewSpawn.DestructableString1 = "";
            //NewSpawn.DestructableString2 = "";
            //NewSpawn.DestructableString3 = "";

            if ((IsNPC > 0) && ((OtherData & 3) > 0))
            {
                // Destructable Objects. Not handled yet
                //
                //SQLOut(String.Format("-- OBJECT FOUND SpawnID {0}", SpawnID.ToString("x")));

                buffer.ReadString(false);

                buffer.ReadString(false);

                buffer.ReadString(false);

                buffer.ReadUInt32();

                buffer.ReadUInt32();

                buffer.ReadUInt32();

                buffer.ReadUInt32();

                buffer.ReadUInt32();

                buffer.ReadUInt32();

                buffer.ReadUInt32();

                buffer.ReadUInt32();

                buffer.ReadUInt32();

                buffer.ReadUInt32();

                buffer.ReadUInt32();

                buffer.ReadUInt32();

                buffer.ReadUInt32();

                buffer.ReadByte();


            }
            outputStream.WriteLine("Size starts at offset {0}", buffer.GetPosition());

            float Size = buffer.ReadSingle();

            byte Face = buffer.ReadByte();

            float WalkSpeed = buffer.ReadSingle();

            float RunSpeed = buffer.ReadSingle();

            UInt32 Race = buffer.ReadUInt32();

            byte PropCount = buffer.ReadByte();

            UInt32 BodyType = 0;

            if (PropCount >= 1)
            {
                BodyType = buffer.ReadUInt32();

                for (int j = 1; j < PropCount; ++j)
                    buffer.SkipBytes(4);
            }

            buffer.SkipBytes(1);   // Skip HP %

            byte HairColor = buffer.ReadByte();
            byte BeardColor = buffer.ReadByte();
            byte EyeColor1 = buffer.ReadByte();
            byte EyeColor2 = buffer.ReadByte();
            byte HairStyle = buffer.ReadByte();
            byte Beard = buffer.ReadByte();

            UInt32 DrakkinHeritage = buffer.ReadUInt32();

            UInt32 DrakkinTattoo = buffer.ReadUInt32();

            UInt32 DrakkinDetails = buffer.ReadUInt32();

            buffer.SkipBytes(1);   // Skip Holding

            UInt32 Deity = buffer.ReadUInt32();

            buffer.SkipBytes(8);    // Skip GuildID and GuildRank

            byte Class = buffer.ReadByte();

            buffer.SkipBytes(4);     // Skip PVP, Standstate, Light, Flymode

            byte EquipChest2 = buffer.ReadByte();

            bool UseWorn = (EquipChest2 == 255);

            buffer.SkipBytes(2);    // 2 Unknown bytes;

            byte Helm = buffer.ReadByte();

            string LastName = buffer.ReadString(true);

            buffer.SkipBytes(5);    // AATitle + unknown byte

            UInt32 PetOwnerID = buffer.ReadUInt32();

            buffer.SkipBytes(26);   // Unknown byte + 6 unknown uint32

            outputStream.WriteLine("Position starts at offset {0}", buffer.GetPosition());

            UInt32 Position1 = buffer.ReadUInt32();

            UInt32 Position2 = buffer.ReadUInt32();

            UInt32 Position3 = buffer.ReadUInt32();

            UInt32 Position4 = buffer.ReadUInt32();

            UInt32 Position5 = buffer.ReadUInt32();

            float YPos = Utils.EQ19ToFloat((Int32)(Position3 & 0x7FFFF));

            float Heading = Utils.EQ19ToFloat((Int32)(Position4 & 0xFFF));

            float XPos = Utils.EQ19ToFloat((Int32)(Position4 >> 12) & 0x7FFFF);

            float ZPos = Utils.EQ19ToFloat((Int32)(Position5 & 0x7FFFF));

            outputStream.WriteLine("(X,Y,Z) = {0}, {1}, {2}, Heading = {3}", XPos, YPos, ZPos, Heading);

            if (NPCType.IsPlayableRace(Race))
            {
                for (int ColourSlot = 0; ColourSlot < 9; ++ColourSlot)
                    outputStream.WriteLine("Color {0} is {1}", ColourSlot, buffer.ReadUInt32());
            }

            outputStream.WriteLine("");

        }

        override public List<ZoneEntryStruct> GetSpawns()
        {
            List<ZoneEntryStruct> ZoneSpawns = new List<ZoneEntryStruct>();

            List<byte[]> SpawnPackets = GetPacketsOfType("OP_ZoneEntry", PacketDirection.ServerToClient);

            foreach (byte[] SpawnPacket in SpawnPackets)
            {
                ZoneEntryStruct NewSpawn = new ZoneEntryStruct();

                ByteStream Buffer = new ByteStream(SpawnPacket);

                NewSpawn.SpawnName = Buffer.ReadString(true);

                NewSpawn.SpawnName = Utils.MakeCleanName(NewSpawn.SpawnName);

                NewSpawn.SpawnID = Buffer.ReadUInt32();

                NewSpawn.Level = Buffer.ReadByte();

                float UnkSize = Buffer.ReadSingle();

                NewSpawn.IsNPC = Buffer.ReadByte();

                UInt32 Bitfield = Buffer.ReadUInt32();

                NewSpawn.Showname = (Bitfield >> 28) & 1;
                NewSpawn.TargetableWithHotkey = (Bitfield >> 27) & 1;
                NewSpawn.Targetable = (Bitfield >> 26) & 1;

                NewSpawn.ShowHelm = (Bitfield >> 24) & 1;
                NewSpawn.Gender = (Bitfield >> 20) & 3;

                NewSpawn.Padding5 = (Bitfield >> 4) & 1;
                NewSpawn.Padding7 = (Bitfield >> 6) & 2047;
                NewSpawn.Padding26 = (Bitfield >> 25) & 1;

                Byte OtherData = Buffer.ReadByte();

                Buffer.SkipBytes(8);    // Skip 8 unknown bytes

                NewSpawn.DestructableString1 = "";
                NewSpawn.DestructableString2 = "";
                NewSpawn.DestructableString3 = "";

                if ((NewSpawn.IsNPC > 0) && ((OtherData & 3) > 0))
                {
                    // Destructable Objects. Not handled yet
                    //
                    //SQLOut(String.Format("-- OBJECT FOUND SpawnID {0}", SpawnID.ToString("x")));

                    NewSpawn.DestructableString1 = Buffer.ReadString(false);

                    NewSpawn.DestructableString2 = Buffer.ReadString(false);

                    NewSpawn.DestructableString3 = Buffer.ReadString(false);

                    NewSpawn.DestructableUnk1 = Buffer.ReadUInt32();

                    NewSpawn.DestructableUnk2 = Buffer.ReadUInt32();

                    NewSpawn.DestructableID1 = Buffer.ReadUInt32();

                    NewSpawn.DestructableID2 = Buffer.ReadUInt32();

                    NewSpawn.DestructableID3 = Buffer.ReadUInt32();

                    NewSpawn.DestructableID4 = Buffer.ReadUInt32();

                    NewSpawn.DestructableUnk3 = Buffer.ReadUInt32();

                    NewSpawn.DestructableUnk4 = Buffer.ReadUInt32();

                    NewSpawn.DestructableUnk5 = Buffer.ReadUInt32();

                    NewSpawn.DestructableUnk6 = Buffer.ReadUInt32();

                    NewSpawn.DestructableUnk7 = Buffer.ReadUInt32();

                    NewSpawn.DestructableUnk8 = Buffer.ReadUInt32();

                    NewSpawn.DestructableUnk9 = Buffer.ReadUInt32();

                    NewSpawn.DestructableByte = Buffer.ReadByte();

                    //SQLOut(String.Format("-- DES: {0,8:x} {1,8:x} {2,8:d} {3,8:d} {4,8:d} {5,8:d} {6,8:x} {7,8:x} {8,8:x} {9,8:x} {10,8:x} {11,8:x} {12,8:x} {13,2:x} {14} {15} {16}",
                    //          DestructableUnk1, DestructableUnk2, DestructableID1, DestructableID2, DestructableID3, DestructableID4,
                    //          DestructableUnk3, DestructableUnk4, DestructableUnk5, DestructableUnk6, DestructableUnk7, DestructableUnk8,
                    //          DestructableUnk9, DestructableByte, DestructableString1, DestructableString2, DestructableString3));
                }

                NewSpawn.Size = Buffer.ReadSingle();

                NewSpawn.Face = Buffer.ReadByte();

                NewSpawn.WalkSpeed = Buffer.ReadSingle();

                NewSpawn.RunSpeed = Buffer.ReadSingle();

                NewSpawn.Race = Buffer.ReadUInt32();

                NewSpawn.PropCount = Buffer.ReadByte();

                NewSpawn.BodyType = 0;

                if (NewSpawn.PropCount >= 1)
                {
                    NewSpawn.BodyType = Buffer.ReadUInt32();

                    for (int j = 1; j < NewSpawn.PropCount; ++j)
                        Buffer.SkipBytes(4);
                }

                Buffer.SkipBytes(1);   // Skip HP %

                NewSpawn.HairColor = Buffer.ReadByte();
                NewSpawn.BeardColor = Buffer.ReadByte();
                NewSpawn.EyeColor1 = Buffer.ReadByte();
                NewSpawn.EyeColor2 = Buffer.ReadByte();
                NewSpawn.HairStyle = Buffer.ReadByte();
                NewSpawn.Beard = Buffer.ReadByte();

                NewSpawn.DrakkinHeritage = Buffer.ReadUInt32();

                NewSpawn.DrakkinTattoo = Buffer.ReadUInt32();

                NewSpawn.DrakkinDetails = Buffer.ReadUInt32();

                Buffer.SkipBytes(1);   // Skip Holding

                NewSpawn.Deity = Buffer.ReadUInt32();

                Buffer.SkipBytes(8);    // Skip GuildID and GuildRank

                NewSpawn.Class = Buffer.ReadByte();

                Buffer.SkipBytes(4);     // Skip PVP, Standstate, Light, Flymode

                NewSpawn.EquipChest2 = Buffer.ReadByte();

                bool UseWorn = (NewSpawn.EquipChest2 == 255);

                Buffer.SkipBytes(2);    // 2 Unknown bytes;

                NewSpawn.Helm = Buffer.ReadByte();

                NewSpawn.LastName = Buffer.ReadString(true);

                Buffer.SkipBytes(5);    // AATitle + unknown byte

                NewSpawn.PetOwnerID = Buffer.ReadUInt32();

                Buffer.SkipBytes(26);   // Unknown byte + 6 unknown uint32

                UInt32 Position1 = Buffer.ReadUInt32();

                UInt32 Position2 = Buffer.ReadUInt32();

                UInt32 Position3 = Buffer.ReadUInt32();

                UInt32 Position4 = Buffer.ReadUInt32();

                UInt32 Position5 = Buffer.ReadUInt32();

                NewSpawn.YPos = Utils.EQ19ToFloat((Int32)(Position3 & 0x7FFFF));

                NewSpawn.Heading = Utils.EQ19ToFloat((Int32)(Position4 & 0xFFF));

                NewSpawn.XPos = Utils.EQ19ToFloat((Int32)(Position4 >> 12) & 0x7FFFF);

                NewSpawn.ZPos = Utils.EQ19ToFloat((Int32)(Position5 & 0x7FFFF));

                NewSpawn.MeleeTexture1 = 0;
                NewSpawn.MeleeTexture2 = 0;

                if (NPCType.IsPlayableRace(NewSpawn.Race))
                {
                    for (int ColourSlot = 0; ColourSlot < 9; ++ColourSlot)
                        NewSpawn.SlotColour[ColourSlot] = Buffer.ReadUInt32();

                    for (int i = 0; i < 9; ++i)
                    {
                        NewSpawn.Equipment[i] = Buffer.ReadUInt32();

                        UInt32 Equip2 = Buffer.ReadUInt32();

                        UInt32 Equip1 = Buffer.ReadUInt32();

                        UInt32 Equip0 = Buffer.ReadUInt32();
                    }

                    if (NewSpawn.Equipment[Constants.MATERIAL_CHEST] > 0)
                    {
                        NewSpawn.EquipChest2 = (byte)NewSpawn.Equipment[Constants.MATERIAL_CHEST];

                    }

                    NewSpawn.ArmorTintRed = (byte)((NewSpawn.SlotColour[Constants.MATERIAL_CHEST] >> 16) & 0xff);

                    NewSpawn.ArmorTintGreen = (byte)((NewSpawn.SlotColour[Constants.MATERIAL_CHEST] >> 8) & 0xff);

                    NewSpawn.ArmorTintBlue = (byte)(NewSpawn.SlotColour[Constants.MATERIAL_CHEST] & 0xff);

                    if (NewSpawn.Equipment[Constants.MATERIAL_PRIMARY] > 0)
                        NewSpawn.MeleeTexture1 = NewSpawn.Equipment[Constants.MATERIAL_PRIMARY];

                    if (NewSpawn.Equipment[Constants.MATERIAL_SECONDARY] > 0)
                        NewSpawn.MeleeTexture2 = NewSpawn.Equipment[Constants.MATERIAL_SECONDARY];

                    if (UseWorn)
                        NewSpawn.Helm = (byte)NewSpawn.Equipment[Constants.MATERIAL_HEAD];
                    else
                        NewSpawn.Helm = 0;

                }
                else
                {
                    // Non playable race

                    Buffer.SkipBytes(16);

                    NewSpawn.MeleeTexture1 = Buffer.ReadUInt32();
                    Buffer.SkipBytes(12);
                    NewSpawn.MeleeTexture2 = Buffer.ReadUInt32();
                    Buffer.SkipBytes(12);
                }

                if (NewSpawn.EquipChest2 == 255)
                    NewSpawn.EquipChest2 = 0;

                if (NewSpawn.Helm == 255)
                    NewSpawn.Helm = 0;

                if ((OtherData & 4) > 0)
                {
                    NewSpawn.Title = Buffer.ReadString(false);
                }

                if ((OtherData & 8) > 0)
                {
                    NewSpawn.Suffix = Buffer.ReadString(false);
                }

                // unknowns
                Buffer.SkipBytes(8);

                NewSpawn.IsMercenary = Buffer.ReadByte();

                ZoneSpawns.Add(NewSpawn);
            }

            return ZoneSpawns;
        }
    }
}
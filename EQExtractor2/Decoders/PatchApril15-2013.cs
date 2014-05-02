using System;
using System.Collections.Generic;
using System.Diagnostics;
using EQExtractor2.Domain;

namespace EQExtractor2.Decoders
{
    class PatchApril152013Decoder : PatchMarch132013Decoder
    {
        public PatchApril152013Decoder()
        {
            Version = "EQ Client Build Date April 15 2013.";

            PatchConfFileName = "patch_April15-2013.conf";

            SupportsSQLGeneration = true;
        }

        override public List<ZoneEntryStruct> GetSpawns()
        {
            List<ZoneEntryStruct> ZoneSpawns = new List<ZoneEntryStruct>();

            List<byte[]> SpawnPackets = GetPacketsOfType("OP_ZoneEntry", PacketDirection.ServerToClient);

            foreach (byte[] SpawnPacket in SpawnPackets)
            {
                ZoneEntryStruct newSpawn = new ZoneEntryStruct();

                ByteStream buffer = new ByteStream(SpawnPacket);

                newSpawn.SpawnName = buffer.ReadString(true);

                newSpawn.SpawnName = Utils.MakeCleanName(newSpawn.SpawnName);

                newSpawn.SpawnID = buffer.ReadUInt32();

                newSpawn.Level = buffer.ReadByte();

                float UnkSize = buffer.ReadSingle();

                newSpawn.IsNPC = buffer.ReadByte();

                UInt32 Bitfield = buffer.ReadUInt32();

                newSpawn.Gender = (Bitfield & 3);

                Byte OtherData = buffer.ReadByte();

                buffer.SkipBytes(8);    // Skip 8 unknown bytes

                newSpawn.DestructableString1 = "";
                newSpawn.DestructableString2 = "";
                newSpawn.DestructableString3 = "";

                if ((newSpawn.IsNPC > 0) && ((OtherData & 1) > 0))
                {
                    // Destructable Objects
                    newSpawn.DestructableString1 = buffer.ReadString(false);
                    newSpawn.DestructableString2 = buffer.ReadString(false);
                    newSpawn.DestructableString3 = buffer.ReadString(false);
                    buffer.SkipBytes(53);
                }

                if ((OtherData & 4) > 0)
                {
                    // Auras
                    buffer.ReadString(false);
                    buffer.ReadString(false);
                    buffer.SkipBytes(54);
                }

                newSpawn.PropCount = buffer.ReadByte();

                if (newSpawn.PropCount > 0)
                    newSpawn.BodyType = buffer.ReadUInt32();
                else
                    newSpawn.BodyType = 0;


                for (int j = 1; j < newSpawn.PropCount; ++j)
                    buffer.SkipBytes(4);

                buffer.SkipBytes(1);   // Skip HP %
                newSpawn.HairColor = buffer.ReadByte();
                newSpawn.BeardColor = buffer.ReadByte();
                newSpawn.EyeColor1 = buffer.ReadByte();
                newSpawn.EyeColor2 = buffer.ReadByte();
                newSpawn.HairStyle = buffer.ReadByte();
                newSpawn.Beard = buffer.ReadByte();

                newSpawn.DrakkinHeritage = buffer.ReadUInt32();
                newSpawn.DrakkinTattoo = buffer.ReadUInt32();
                newSpawn.DrakkinDetails = buffer.ReadUInt32();

                newSpawn.EquipChest2 = buffer.ReadByte();

                bool UseWorn = (newSpawn.EquipChest2 == 255);

                buffer.SkipBytes(2);    // 2 Unknown bytes;

                newSpawn.Helm = buffer.ReadByte();

                newSpawn.Size = buffer.ReadSingle();

                newSpawn.Face = buffer.ReadByte();

                newSpawn.WalkSpeed = buffer.ReadSingle();

                newSpawn.RunSpeed = buffer.ReadSingle();

                newSpawn.Race = buffer.ReadUInt32();

                buffer.SkipBytes(1);   // Skip Holding

                newSpawn.Deity = buffer.ReadUInt32();

                buffer.SkipBytes(8);    // Skip GuildID and GuildRank

                newSpawn.Class = buffer.ReadByte();

                buffer.SkipBytes(4);     // Skip PVP, Standstate, Light, Flymode

                newSpawn.LastName = buffer.ReadString(true);

                buffer.SkipBytes(6);

                newSpawn.PetOwnerID = buffer.ReadUInt32();

                buffer.SkipBytes(25);

                newSpawn.MeleeTexture1 = 0;
                newSpawn.MeleeTexture2 = 0;

                if ((newSpawn.IsNPC == 0) || NPCType.IsPlayableRace(newSpawn.Race))
                {
                    for (int ColourSlot = 0; ColourSlot < 9; ++ColourSlot)
                        newSpawn.SlotColour[ColourSlot] = buffer.ReadUInt32();

                    for (int i = 0; i < 9; ++i)
                    {
                        newSpawn.Equipment[i] = buffer.ReadUInt32();

                        UInt32 Equip3 = buffer.ReadUInt32();

                        UInt32 Equip2 = buffer.ReadUInt32();

                        UInt32 Equip1 = buffer.ReadUInt32();

                        UInt32 Equip0 = buffer.ReadUInt32();
                    }

                    if (newSpawn.Equipment[Constants.MATERIAL_CHEST] > 0)
                    {
                        newSpawn.EquipChest2 = (byte)newSpawn.Equipment[Constants.MATERIAL_CHEST];

                    }

                    newSpawn.ArmorTintRed = (byte)((newSpawn.SlotColour[Constants.MATERIAL_CHEST] >> 16) & 0xff);

                    newSpawn.ArmorTintGreen = (byte)((newSpawn.SlotColour[Constants.MATERIAL_CHEST] >> 8) & 0xff);

                    newSpawn.ArmorTintBlue = (byte)(newSpawn.SlotColour[Constants.MATERIAL_CHEST] & 0xff);

                    if (newSpawn.Equipment[Constants.MATERIAL_PRIMARY] > 0)
                        newSpawn.MeleeTexture1 = newSpawn.Equipment[Constants.MATERIAL_PRIMARY];

                    if (newSpawn.Equipment[Constants.MATERIAL_SECONDARY] > 0)
                        newSpawn.MeleeTexture2 = newSpawn.Equipment[Constants.MATERIAL_SECONDARY];

                    if (UseWorn)
                        newSpawn.Helm = (byte)newSpawn.Equipment[Constants.MATERIAL_HEAD];
                    else
                        newSpawn.Helm = 0;

                }
                else
                {
                    // Non playable race

                    buffer.SkipBytes(20);

                    newSpawn.MeleeTexture1 = buffer.ReadUInt32();
                    buffer.SkipBytes(16);
                    newSpawn.MeleeTexture2 = buffer.ReadUInt32();
                    buffer.SkipBytes(16);
                }

                if (newSpawn.EquipChest2 == 255)
                    newSpawn.EquipChest2 = 0;

                if (newSpawn.Helm == 255)
                    newSpawn.Helm = 0;

                UInt32 Position1 = buffer.ReadUInt32();

                UInt32 Position2 = buffer.ReadUInt32();

                UInt32 Position3 = buffer.ReadUInt32();

                UInt32 Position4 = buffer.ReadUInt32();

                UInt32 Position5 = buffer.ReadUInt32();

                newSpawn.YPos = Utils.EQ19ToFloat((Int32)((Position1 >> 12) & 0x7FFFF));

                newSpawn.ZPos = Utils.EQ19ToFloat((Int32)(Position2) & 0x7FFFF);

                newSpawn.XPos = Utils.EQ19ToFloat((Int32)(Position4 >> 13) & 0x7FFFF);

                newSpawn.Heading = Utils.EQ19ToFloat((Int32)(Position3 >> 13) & 0xFFF);

                if ((OtherData & 16) > 0)
                {
                    newSpawn.Title = buffer.ReadString(false);
                }

                if ((OtherData & 32) > 0)
                {
                    newSpawn.Suffix = buffer.ReadString(false);
                }

                // unknowns
                buffer.SkipBytes(8);

                newSpawn.IsMercenary = buffer.ReadByte();

                buffer.SkipBytes(54);
                var expectedLength = buffer.Length();
                var currentPoint = buffer.GetPosition();
                Debug.Assert(currentPoint == expectedLength, "Length mismatch while parsing zone spawns");

                ZoneSpawns.Add(newSpawn);
            }
            return ZoneSpawns;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using EQExtractor2.Domain;

namespace EQExtractor2.Decoders
{
    class PatchJanuary162013Decoder : PatchDecember102012Decoder
    {
        public PatchJanuary162013Decoder()
        {
            Version = "EQ Client Build Date January 16 2013.";

            PatchConfFileName = "patch_Jan16-2013.conf";
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

                NewSpawn.Gender = (Bitfield & 3);

                Byte OtherData = Buffer.ReadByte();

                Buffer.SkipBytes(8);    // Skip 8 unknown bytes

                NewSpawn.DestructableString1 = "";
                NewSpawn.DestructableString2 = "";
                NewSpawn.DestructableString3 = "";

                if ((NewSpawn.IsNPC > 0) && ((OtherData & 1) > 0))
                {
                    // Destructable Objects
                    NewSpawn.DestructableString1 = Buffer.ReadString(false);
                    NewSpawn.DestructableString2 = Buffer.ReadString(false);
                    NewSpawn.DestructableString3 = Buffer.ReadString(false);
                    Buffer.SkipBytes(53);
                }

                if ((OtherData & 4) > 0)
                {
                    // Auras
                    Buffer.ReadString(false);
                    Buffer.ReadString(false);
                    Buffer.SkipBytes(54);
                }

                NewSpawn.PropCount = Buffer.ReadByte();

                if (NewSpawn.PropCount > 0)
                    NewSpawn.BodyType = Buffer.ReadUInt32();
                else
                    NewSpawn.BodyType = 0;


                for (int j = 1; j < NewSpawn.PropCount; ++j)
                    Buffer.SkipBytes(4);

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

                NewSpawn.EquipChest2 = Buffer.ReadByte();

                bool UseWorn = (NewSpawn.EquipChest2 == 255);

                Buffer.SkipBytes(2);    // 2 Unknown bytes;

                NewSpawn.Helm = Buffer.ReadByte();

                NewSpawn.Size = Buffer.ReadSingle();

                NewSpawn.Face = Buffer.ReadByte();

                NewSpawn.WalkSpeed = Buffer.ReadSingle();

                NewSpawn.RunSpeed = Buffer.ReadSingle();

                NewSpawn.Race = Buffer.ReadUInt32();

                Buffer.SkipBytes(1);   // Skip Holding

                NewSpawn.Deity = Buffer.ReadUInt32();

                Buffer.SkipBytes(8);    // Skip GuildID and GuildRank

                NewSpawn.Class = Buffer.ReadByte();

                Buffer.SkipBytes(4);     // Skip PVP, Standstate, Light, Flymode

                NewSpawn.LastName = Buffer.ReadString(true);

                Buffer.SkipBytes(6);

                NewSpawn.PetOwnerID = Buffer.ReadUInt32();

                Buffer.SkipBytes(25);

                NewSpawn.MeleeTexture1 = 0;
                NewSpawn.MeleeTexture2 = 0;

                if ((NewSpawn.IsNPC == 0) || NPCType.IsPlayableRace(NewSpawn.Race))
                {
                    for (int ColourSlot = 0; ColourSlot < 9; ++ColourSlot)
                        NewSpawn.SlotColour[ColourSlot] = Buffer.ReadUInt32();

                    for (int i = 0; i < 9; ++i)
                    {
                        NewSpawn.Equipment[i] = Buffer.ReadUInt32();

                        UInt32 Equip3 = Buffer.ReadUInt32();

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

                    Buffer.SkipBytes(20);

                    NewSpawn.MeleeTexture1 = Buffer.ReadUInt32();
                    Buffer.SkipBytes(16);
                    NewSpawn.MeleeTexture2 = Buffer.ReadUInt32();
                    Buffer.SkipBytes(16);
                }

                if (NewSpawn.EquipChest2 == 255)
                    NewSpawn.EquipChest2 = 0;

                if (NewSpawn.Helm == 255)
                    NewSpawn.Helm = 0;

                UInt32 Position1 = Buffer.ReadUInt32();

                UInt32 Position2 = Buffer.ReadUInt32();

                UInt32 Position3 = Buffer.ReadUInt32();

                UInt32 Position4 = Buffer.ReadUInt32();

                UInt32 Position5 = Buffer.ReadUInt32();

                NewSpawn.YPos = Utils.EQ19ToFloat((Int32)((Position4 >> 13) & 0x7FFFF));

                NewSpawn.ZPos = Utils.EQ19ToFloat((Int32)(Position5 >> 10) & 0x7FFFF);

                NewSpawn.XPos = Utils.EQ19ToFloat((Int32)(Position2 >> 10) & 0x7FFFF);

                NewSpawn.Heading = Utils.EQ19ToFloat((Int32)(Position3) & 0xFFF);

                if ((OtherData & 16) > 0)
                {
                    NewSpawn.Title = Buffer.ReadString(false);
                }

                if ((OtherData & 32) > 0)
                {
                    NewSpawn.Suffix = Buffer.ReadString(false);
                }

                // unknowns
                Buffer.SkipBytes(8);

                NewSpawn.IsMercenary = Buffer.ReadByte();

                Buffer.SkipBytes(54);

                Debug.Assert(Buffer.GetPosition() == Buffer.Length(), "Length mismatch while parsing zone spawns");

                ZoneSpawns.Add(NewSpawn);
            }

            return ZoneSpawns;
        }

        public override void RegisterExplorers()
        {
            //OpManager.RegisterExplorer("OP_ZoneEntry", ExploreZoneEntry);
            //OpManager.RegisterExplorer("OP_NPCMoveUpdate", ExploreNPCMoveUpdate);
            //OpManager.RegisterExplorer("OP_MobUpdate", ExploreMobUpdate);
        }

        public override void ExploreNPCMoveUpdate(StreamWriter OutputStream, ByteStream Buffer, PacketDirection Direction)
        {
            PositionUpdate PosUpdate;

            PosUpdate = Decode_OP_NPCMoveUpdate(Buffer.Buffer);

            OutputStream.WriteLine("OP_NPCMoveUpdate SpawnID: {0}, X = {1}, Y = {2}, Z = {3}, Heading = {4}", PosUpdate.SpawnID, PosUpdate.p.x, PosUpdate.p.y, PosUpdate.p.z, PosUpdate.p.heading);
        }

        public override void ExploreMobUpdate(StreamWriter OutputStream, ByteStream Buffer, PacketDirection Direction)
        {
            PositionUpdate PosUpdate;

            PosUpdate = Decode_OP_MobUpdate(Buffer.Buffer);

            OutputStream.WriteLine("OP_MobUpdate SpawnID: {0}, X = {1}, Y = {2}, Z = {3}, Heading = {4}", PosUpdate.SpawnID, PosUpdate.p.x, PosUpdate.p.y, PosUpdate.p.z, PosUpdate.p.heading);
        }

        public override void ExploreZoneEntry(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            if (direction != PacketDirection.ServerToClient)
                return;

            string FirstName = buffer.ReadString(false);

            outputStream.WriteLine("Name = {0}", FirstName);

            UInt32 SpawnID = buffer.ReadUInt32();

            outputStream.WriteLine("SpawnID = {0}", SpawnID);

            byte Level = buffer.ReadByte();

            outputStream.WriteLine("Level = {0}", Level);

            buffer.SkipBytes(4);

            byte IsNPC = buffer.ReadByte();

            outputStream.WriteLine("IsNPC = {0}", IsNPC);

            UInt32 Bitfield = buffer.ReadUInt32();
            outputStream.WriteLine("Name: {0}, Bitfield: {1}", FirstName, Convert.ToString(Bitfield, 2));

            byte OtherData = buffer.ReadByte();

            outputStream.WriteLine("OtherData = {0}", OtherData);

            buffer.SkipBytes(8);

            if ((OtherData & 1) > 0)
            {
                outputStream.WriteLine("OD:     {0}", buffer.ReadString(false));
                outputStream.WriteLine("OD:     {0}", buffer.ReadString(false));
                outputStream.WriteLine("OD:     {0}", buffer.ReadString(false));
                buffer.SkipBytes(53);
            }

            if ((OtherData & 4) > 0)
            {
                outputStream.WriteLine("Aura:     {0}", buffer.ReadString(false));
                outputStream.WriteLine("Aura:     {0}", buffer.ReadString(false));
                buffer.SkipBytes(54);
            }

            byte Properties = buffer.ReadByte();
            outputStream.WriteLine("Properties = {0}, Offset now {1}", Properties, buffer.GetPosition());

            UInt32 BodyType = 0;

            if (Properties > 0)
                BodyType = buffer.ReadUInt32();

            outputStream.WriteLine("Bodytype = {0}", BodyType);

            if (Properties != 1)
                outputStream.WriteLine("XXXX Properties is {0}", Properties);

            for (int i = 1; i < Properties; ++i)
                outputStream.WriteLine("   Prop: {0}", buffer.ReadUInt32());

            outputStream.WriteLine("Position is now {0}", buffer.GetPosition());

            byte HP = buffer.ReadByte();
            byte HairColor = buffer.ReadByte();
            byte BeardColor = buffer.ReadByte();
            byte Eye1 = buffer.ReadByte();
            byte Eye2 = buffer.ReadByte();
            byte HairStyle = buffer.ReadByte();
            byte BeardStyle = buffer.ReadByte();
            outputStream.WriteLine("Beardstyle is {0}", BeardStyle);

            buffer.SkipBytes(12);   // Drakkin stuff
            byte EquipChest2 = buffer.ReadByte();
            buffer.SkipBytes(2);
            byte Helm = buffer.ReadByte();


            float Size = buffer.ReadSingle();

            byte Face = buffer.ReadByte();

            float WalkSpeed = buffer.ReadSingle();

            float RunSpeed = buffer.ReadSingle();

            UInt32 Race = buffer.ReadUInt32();

            outputStream.WriteLine("Size: {0}, Face: {1}, Walkspeed: {2}, RunSpeed: {3}, Race: {4}", Size, Face, WalkSpeed, RunSpeed, Race);

            buffer.SkipBytes(18);

            buffer.ReadString(false);

            buffer.SkipBytes(35);


            if ((IsNPC == 0) || NPCType.IsPlayableRace(Race))
            {
                for (int ColourSlot = 0; ColourSlot < 9; ++ColourSlot)
                    outputStream.WriteLine("Color {0} is {1}", ColourSlot, buffer.ReadUInt32());

                for (int i = 0; i < 9; ++i)
                {
                    UInt32 Equip3 = buffer.ReadUInt32();

                    UInt32 Equipx = buffer.ReadUInt32();

                    UInt32 Equip2 = buffer.ReadUInt32();

                    UInt32 Equip1 = buffer.ReadUInt32();

                    UInt32 Equip0 = buffer.ReadUInt32();

                    outputStream.WriteLine("Equip slot {0}: 0,1,2,x,3  is {1}, {2}, {3}, {4}, {5}", i,
                        Equip0, Equip1, Equip2, Equipx, Equip3);
                }





            }
            else
            {
                // Non playable race
                // Melee Texture 1 is 20 bytes in
                // Melee Texture 1 is 40 bytes in
                // This whole segment is 28 + 24 + 8 = 60
                // Skip 20, Read m1, skip 16, read m2, skip 16
                /*
                OutputStream.WriteLine("None playable race,  offset now {0}", Buffer.GetPosition());
                Buffer.SkipBytes(28);

                UInt32 MeleeTexture1 = Buffer.ReadUInt32();
                Buffer.SkipBytes(12);
                UInt32 MeleeTexture2 = Buffer.ReadUInt32();
                Buffer.SkipBytes(12);
                 */
                outputStream.WriteLine("None playable race,  offset now {0}", buffer.GetPosition());
                buffer.SkipBytes(20);

                UInt32 MeleeTexture1 = buffer.ReadUInt32();
                buffer.SkipBytes(16);
                UInt32 MeleeTexture2 = buffer.ReadUInt32();
                buffer.SkipBytes(16);
            }

            outputStream.WriteLine("Position starts at offset {0}", buffer.GetPosition());

            UInt32 Position1 = buffer.ReadUInt32();

            UInt32 Position2 = buffer.ReadUInt32();

            UInt32 Position3 = buffer.ReadUInt32();

            UInt32 Position4 = buffer.ReadUInt32();

            UInt32 Position5 = buffer.ReadUInt32();

            float YPos = Utils.EQ19ToFloat((Int32)((Position4 >> 13) & 0x7FFFF));

            float ZPos = Utils.EQ19ToFloat((Int32)(Position5 >> 10) & 0x7FFFF);

            float XPos = Utils.EQ19ToFloat((Int32)(Position2 >> 10) & 0x7FFFF);

            //float Heading = Utils.EQ19ToFloat((Int32)(Position1 >> 13) & 0x3FF);
            //float Heading = Utils.EQ19ToFloat((Int32)(Position2) & 0x3FF);
            float Heading = Utils.EQ19ToFloat((Int32)(Position3) & 0x3FF);

            //for(int i = 0; i < 32; ++i)
            //   OutputStream.WriteLine("Pos3 << {0} = {1}", i, Utils.EQ19ToFloat((Int32)(Position3 >> i) & 0x3FF));

            outputStream.WriteLine("(X,Y,Z) = {0}, {1}, {2}, Heading = {3}", XPos, YPos, ZPos, Heading);

            if ((OtherData & 16) > 1)
                outputStream.WriteLine("Title: {0}", buffer.ReadString(false));

            if ((OtherData & 32) > 1)
                outputStream.WriteLine("Suffix: {0}", buffer.ReadString(false));

            buffer.SkipBytes(8);

            byte IsMerc = buffer.ReadByte();

            outputStream.WriteLine("IsMerc: {0}", IsMerc);

            buffer.SkipBytes(54);

            outputStream.WriteLine("Buffer Length: {0}, Current Position: {1}", buffer.Length(), buffer.GetPosition());

            if (buffer.Length() != buffer.GetPosition())
                outputStream.WriteLine("PARSE ERROR");





            outputStream.WriteLine("");
        }
    }
}
using System;

namespace EQExtractor2.Decoders
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.IO;
    using EQExtractor2.Domain;

    class PatchMay192014Decoder: PatchApr242014Decoder
    {
        public PatchMay192014Decoder()
        {
            Version = "EQ Client Build Date May 19 2014.";

            PatchConfFileName = "patch_May19-2014.conf";

            SupportsSQLGeneration = true;
        }

        public override IdentificationStatus Identify(int opCode, int size, PacketDirection direction)
        {
            var opZoneEntry=OpManager.OpCodeNameToNumber("OP_ZoneEntry") ;
            var opPlayerProfile = OpManager.OpCodeNameToNumber("OP_PlayerProfile");
            if (opCode == opZoneEntry && direction == PacketDirection.ClientToServer)
                return IdentificationStatus.Tentative;

            if (opCode == opPlayerProfile && direction == PacketDirection.ServerToClient) // &&(Size == ExpectedPPLength))
                return IdentificationStatus.Yes;


            return IdentificationStatus.No;
        }

        public override void RegisterExplorers()
        {
            //OpManager.RegisterExplorer("OP_PlayerProfile", ExplorePlayerProfile);
            OpManager.RegisterExplorer("OP_ZoneEntry", ExploreZoneEntry);
            OpManager.RegisterExplorer("OP_NPCMoveUpdate", ExploreNPCMoveUpdate);
            OpManager.RegisterExplorer("OP_MobUpdate", ExploreMobUpdate);
            //OpManager.RegisterExplorer("OP_ClientUpdate", ExploreClientUpdate);
           // OpManager.RegisterExplorer("OP_CharInventory", ExploreInventory);
        }

        public override void ExploreZoneEntry(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            if (direction != PacketDirection.ServerToClient)
                return;
            try
            {
                var firstName = buffer.ReadString(false); //verified
                outputStream.WriteLine("Name = {0}", firstName);
                //File.WriteAllBytes(string.Format("{0}.bin",Utils.MakeCleanName(firstName)),buffer.Buffer);
                var spawnId = buffer.ReadUInt32();//verified
                outputStream.WriteLine("SpawnID = {0}", spawnId);
                var level = buffer.ReadByte();//verified
                outputStream.WriteLine("Level = {0}", level);
                buffer.SkipBytes(4);
                var isNpc = buffer.ReadByte();//verified
                outputStream.WriteLine("IsNPC = {0}", isNpc);
                var bitfield = buffer.ReadUInt32();
                outputStream.WriteLine("Name: {0}, Bitfield: {1}", firstName, Convert.ToString(bitfield, 2));
                outputStream.WriteLine("Gender: {0}", (bitfield & 3));//verified
                var otherData = buffer.ReadByte();
                outputStream.WriteLine("OtherData = {0}", otherData);
                buffer.SkipBytes(8);
                // otherdata stuff is unverified
                if ((otherData & 1) > 0)
                {
                    outputStream.WriteLine("OD:     {0}", buffer.ReadString(false));
                    outputStream.WriteLine("OD:     {0}", buffer.ReadString(false));
                    outputStream.WriteLine("OD:     {0}", buffer.ReadString(false));
                    buffer.SkipBytes(53);
                }

                if ((otherData & 4) > 0)
                {
                    outputStream.WriteLine("Aura:     {0}", buffer.ReadString(false));
                    outputStream.WriteLine("Aura:     {0}", buffer.ReadString(false));
                    buffer.SkipBytes(54);
                }

                //properties unverified in the sense that I don't know if they represent anything useful other than bodytype
                var properties = buffer.ReadByte();
                outputStream.WriteLine("Properties = {0}, Offset now {1}", properties, buffer.GetPosition());

                UInt32 bodyType = 0;

                if (properties > 0)
                    bodyType = buffer.ReadUInt32(); //verified

                outputStream.WriteLine("Bodytype = {0}", bodyType);

                if (properties != 1)
                    outputStream.WriteLine("XXXX Properties is {0}", properties);

                for (var i = 1; i < properties; ++i)
                    outputStream.WriteLine("   Prop: {0}", buffer.ReadUInt32());

                var hp = buffer.ReadByte(); //not 100% sure this is HP. I got 47% on my character when her hp at 100%. Poss mana?
                //Below here is verified
                var beardStyle = buffer.ReadByte();
                outputStream.WriteLine("Beardstyle is {0}", beardStyle);
                var hairColor = buffer.ReadByte();
                outputStream.WriteLine("Hair color is {0}", hairColor);
                var eye1 = buffer.ReadByte();
                outputStream.WriteLine("Eye1 is {0}", eye1);
                var eye2 = buffer.ReadByte();
                outputStream.WriteLine("Eye2 is {0}", eye2);
                var hairStyle = buffer.ReadByte();
                outputStream.WriteLine("Hair style is {0}", hairStyle);
                var beardColor = buffer.ReadByte();
                outputStream.WriteLine("Beard color is {0}", beardColor);

                var drakkinHeritage = buffer.ReadUInt32();
                outputStream.WriteLine("Drakkin Heritage is {0}", drakkinHeritage);
                // an_unemployed_mercenary's and some newer npc's seem to have this set to 255, then have invalid numbers for the next ones
                if (drakkinHeritage == 255)
                {
                    outputStream.WriteLine("We should set drakkinHeritage to 0 as well as the other Drakkin stuff.");
                    outputStream.WriteLine("Drakkin Heritage is 0");
                    outputStream.WriteLine("Drakkin Tattoo is 0");
                    outputStream.WriteLine("Drakkin Details is 0");
                    buffer.SkipBytes(8);
                }
                else
                {
                    outputStream.WriteLine("Drakkin Tattoo is {0}", buffer.ReadUInt32());
                    outputStream.WriteLine("Drakkin Details is {0}", buffer.ReadUInt32());
                }

                var equipChest2 = buffer.ReadByte(); //AKA texture
                var useWorn = equipChest2 == 255;
                buffer.SkipBytes(2);
                var helm = buffer.ReadByte(); //unverified
                var size = buffer.ReadSingle(); //verified
                var face = buffer.ReadByte(); // Probably correct
                var walkSpeed = buffer.ReadSingle();   //dunno valid ranges for this so this is anyone's guess :P 
                var runSpeed = buffer.ReadSingle(); // verified
                var race = buffer.ReadUInt32(); //verified
                //dunno about bits below here
                outputStream.WriteLine("Holding = {0}", buffer.ReadByte());
                outputStream.WriteLine("Deity = {0}", buffer.ReadUInt32()); //verified
                outputStream.WriteLine("GuildID = {0}", buffer.ReadUInt32());//unverified
                outputStream.WriteLine("Guildstatus = {0}", buffer.ReadUInt32());//unverified
                outputStream.WriteLine("Class = {0}", buffer.ReadUInt32());//verified
                outputStream.WriteLine("Size: {0}, Face: {1}, Walkspeed: {2}, RunSpeed: {3}, Race: {4}", size, face, walkSpeed, runSpeed, race);
                buffer.SkipBytes(1); //PVP-//unverified
                outputStream.WriteLine("Stand State = {0}", buffer.ReadByte());//unverified
                outputStream.WriteLine("Light = {0}", buffer.ReadByte());//unverified
                buffer.SkipBytes(1); //Flymode! //unverified
                var lastName = buffer.ReadString(false);
                outputStream.WriteLine("LastName = {0}", lastName); //verified
                buffer.SkipBytes(6);
                outputStream.WriteLine("PetOwnerId = {0}", buffer.ReadUInt32());//unverified
                buffer.SkipBytes(isNpc == 1 ? 37 : 25);

                if (isNpc == 0 || NPCType.IsPlayableRace(race))
                {
                    var posn = buffer.GetPosition();
                    for (int ColourSlot = 0; ColourSlot < 9; ++ColourSlot)
                        outputStream.WriteLine("Color {0} is {1}", ColourSlot, buffer.ReadUInt32());
                    var diff = buffer.GetPosition() - posn;
                    Debug.Assert(diff == 36, "Colour slots wrong!");
                    //Player equip verified

                    for (var i = 0; i < 9; ++i)
                    {
                        var equip3 = buffer.ReadUInt32();

                        var equipx = buffer.ReadUInt32();

                        var equip2 = buffer.ReadUInt32();

                        var equip1 = buffer.ReadUInt32();

                        var equip0 = buffer.ReadUInt32();

                        outputStream.WriteLine("Equip slot {0}: 0,1,2,x,3  is {1}, {2}, {3}, {4}, {5}", i,
                            equip0, equip1, equip2, equipx, equip3);
                    }
                }
                else
                {
                    //vsab at this point this section is 100% untested

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

                    var meleeTexture1 = buffer.ReadUInt32();
                    buffer.SkipBytes(16);
                    var meleeTexture2 = buffer.ReadUInt32();
                    buffer.SkipBytes(16);
                }

                //positions verified!
                outputStream.WriteLine("Position starts at offset {0}", buffer.GetPosition());

                var position1 = buffer.ReadUInt32();
                outputStream.WriteLine("Position1 untreated {0}", position1);
                var position2 = buffer.ReadUInt32();
                outputStream.WriteLine("Position2 untreated {0}", position2);
                var position3 = buffer.ReadUInt32();
                outputStream.WriteLine("Position3 untreated {0}", position3);
                var position4 = buffer.ReadUInt32();
                outputStream.WriteLine("Position4 untreated {0}", position4);

                var position5 = buffer.ReadUInt32();
                outputStream.WriteLine("Position5 untreated {0}", position5);
          
                var spawnPos = GetSpawnPosition(position1, position2, position3, position4, position5);

                outputStream.WriteLine("(X,Y,Z) = {0}, {1}, {2}, Heading = {3}", spawnPos.X, spawnPos.Y, spawnPos.Z, spawnPos.Heading);

                if ((otherData & 16) > 1)
                    outputStream.WriteLine("Title: {0}", buffer.ReadString(false)); //verified

                if ((otherData & 32) > 1)
                    outputStream.WriteLine("Suffix: {0}", buffer.ReadString(false)); //verified

                buffer.SkipBytes(8);

                var isMerc = buffer.ReadByte(); //verified

                outputStream.WriteLine("IsMerc: {0}", isMerc);

                buffer.SkipBytes(54);
                var expectedLength = buffer.Length();
                var currentPoint = buffer.GetPosition();
                outputStream.WriteLine("Buffer Length: {0}, Current Position: {1}", expectedLength, currentPoint);
                Debug.Assert(currentPoint == expectedLength, "Length mismatch while parsing zone spawns");
                outputStream.WriteLine("");
            }
            catch (Exception)
            {
            }
        }


        override public List<ZoneEntryStruct> GetSpawns()
        {
            List<ZoneEntryStruct> zoneSpawns = new List<ZoneEntryStruct>();

            List<byte[]> spawnPackets = GetPacketsOfType("OP_ZoneEntry", PacketDirection.ServerToClient);

            foreach (var spawnPacket in spawnPackets)
            {
                var newSpawn = new ZoneEntryStruct();

                var buffer = new ByteStream(spawnPacket);

                newSpawn.SpawnName = buffer.ReadString(true);
                newSpawn.SpawnName = Utils.MakeCleanName(newSpawn.SpawnName);
                newSpawn.SpawnID = buffer.ReadUInt32();
                newSpawn.Level = buffer.ReadByte();
                buffer.SkipBytes(4);
                newSpawn.IsNPC = buffer.ReadByte();
                var bitfield = buffer.ReadUInt32();
                newSpawn.Gender = (bitfield & 3);
                var otherData = buffer.ReadByte();
                buffer.SkipBytes(8);

                if ((otherData & 1) > 0)
                {
                    newSpawn.DestructableString1 = buffer.ReadString(false);
                    newSpawn.DestructableString2 = buffer.ReadString(false);
                    newSpawn.DestructableString3 = buffer.ReadString(false);
                    buffer.SkipBytes(53);
                }

                if ((otherData & 4) > 0)
                {
                    buffer.ReadString(false);
                    buffer.ReadString(false);
                    buffer.SkipBytes(54);
                }

                newSpawn.PropCount = buffer.ReadByte();

                if (newSpawn.PropCount > 0)
                    newSpawn.BodyType = buffer.ReadUInt32();
                else
                    newSpawn.BodyType = 0;

                for (var i = 1; i < newSpawn.PropCount; ++i)
                    buffer.SkipBytes(4);

                buffer.SkipBytes(1);   // Skip HP %
                newSpawn.Beard = buffer.ReadByte(); //Beardstyle
                newSpawn.HairColor = buffer.ReadByte();
                newSpawn.EyeColor1 = buffer.ReadByte();
                newSpawn.EyeColor2 = buffer.ReadByte();
                newSpawn.HairStyle = buffer.ReadByte();
                newSpawn.BeardColor = buffer.ReadByte();


                newSpawn.DrakkinHeritage = buffer.ReadUInt32();
                // vsab: an_unemployed_mercenary's and some newer npc's seem to have newSpawn.DrakkinHeritage set to 255, then have invalid numbers for the next ones
                if (newSpawn.DrakkinHeritage == 255)
                {
                    newSpawn.DrakkinHeritage = 0;
                    newSpawn.DrakkinTattoo = 0;
                    newSpawn.DrakkinDetails = 0;
                    buffer.SkipBytes(8);
                }
                else
                {
                    newSpawn.DrakkinTattoo = buffer.ReadUInt32();
                    newSpawn.DrakkinDetails = buffer.ReadUInt32();
                }

                newSpawn.EquipChest2 = buffer.ReadByte();
                var useWorn = (newSpawn.EquipChest2 == 255);
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
                newSpawn.Class = buffer.ReadUInt32();
                buffer.SkipBytes(1);     // Skip PVP, Standstate, Light, Flymode
                newSpawn.StandState = buffer.ReadByte(); //110 is sitting, 100 is standing, 120 is FD/corpse, mounts show as 102
                buffer.SkipBytes(2);
                newSpawn.LastName = buffer.ReadString(true);

                buffer.SkipBytes(6);

                newSpawn.PetOwnerID = buffer.ReadUInt32();

                newSpawn.MeleeTexture1 = 0;
                newSpawn.MeleeTexture2 = 0;
                buffer.SkipBytes(newSpawn.IsNPC == 1 ? 37 : 25);

                if (newSpawn.IsNPC == 0 || NPCType.IsPlayableRace(newSpawn.Race))
                {
                    var posn = buffer.GetPosition();
                    for (var colourSlot = 0; colourSlot < 9; ++colourSlot)
                        newSpawn.SlotColour[colourSlot] = buffer.ReadUInt32();
                    var diff = buffer.GetPosition() - posn;
                    Debug.Assert(diff == 36, "Colour slots wrong!");
                    for (var i = 0; i < 9; ++i)
                    {
                        newSpawn.Equipment[i] = buffer.ReadUInt32();

                        var equip3 = buffer.ReadUInt32();

                        var equip2 = buffer.ReadUInt32();

                        var equip1 = buffer.ReadUInt32();

                        var equip0 = buffer.ReadUInt32();
                    }

                    if (newSpawn.Equipment[Constants.MATERIAL_CHEST] > 0)
                    {
                        newSpawn.EquipChest2 = (byte)newSpawn.Equipment[Constants.MATERIAL_CHEST];

                    }
                    //vsab: unverified.....
                    newSpawn.ArmorTintRed = (byte)((newSpawn.SlotColour[Constants.MATERIAL_CHEST] >> 16) & 0xff);
                    newSpawn.ArmorTintGreen = (byte)((newSpawn.SlotColour[Constants.MATERIAL_CHEST] >> 8) & 0xff);
                    newSpawn.ArmorTintBlue = (byte)(newSpawn.SlotColour[Constants.MATERIAL_CHEST] & 0xff);

                    if (newSpawn.Equipment[Constants.MATERIAL_PRIMARY] > 0)
                        newSpawn.MeleeTexture1 = newSpawn.Equipment[Constants.MATERIAL_PRIMARY];

                    if (newSpawn.Equipment[Constants.MATERIAL_SECONDARY] > 0)
                        newSpawn.MeleeTexture2 = newSpawn.Equipment[Constants.MATERIAL_SECONDARY];

                    if (useWorn)
                        newSpawn.Helm = (byte)newSpawn.Equipment[Constants.MATERIAL_HEAD];
                    else
                        newSpawn.Helm = 0;
                }
                else
                {
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


                var position1 = buffer.ReadUInt32();
                var position2 = buffer.ReadUInt32();
                var position3 = buffer.ReadUInt32();
                var position4 = buffer.ReadUInt32();
                var position5 = buffer.ReadUInt32();
                var spawnPos = GetSpawnPosition(position1, position2, position3, position4, position5);

                newSpawn.XPos = spawnPos.X;
                newSpawn.YPos = spawnPos.Y;
                newSpawn.ZPos = spawnPos.Z;
                newSpawn.Heading = spawnPos.Heading;

                if ((otherData & 16) > 1)
                {
                    newSpawn.Title = buffer.ReadString(false);
                }

                if ((otherData & 32) > 1)
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
                zoneSpawns.Add(newSpawn);
            }
            return zoneSpawns;
        }

        protected virtual SpawnPosition GetSpawnPosition(UInt32 p1, UInt32 p2, UInt32 p3, UInt32 p4, UInt32 p5)
        {
            var posn = new SpawnPosition();
            posn.X = Utils.EQ19ToFloat((Int32)(p5 >> 12) & 0x7FFFF); //!
            posn.Y = Utils.EQ19ToFloat((Int32)(p4) & 0x7FFFF); //!
            posn.Z = Utils.EQ19ToFloat((Int32)((p1 >> 12) & 0x7FFFF)); //!
            posn.Heading = Utils.EQ19ToFloat((Int32)(p5) & 0xFFF);
            return posn;
        }
    }
}

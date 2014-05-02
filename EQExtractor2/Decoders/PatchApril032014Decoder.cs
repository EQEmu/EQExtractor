using System;
using System.Collections.Generic;
using System.IO;
using EQExtractor2.Domain;

namespace EQExtractor2.Decoders
{
    internal class PatchApril032014Decoder : PatchApril152013Decoder, IPatchDecoder
    {
        public PatchApril032014Decoder()
        {
            Version = "EQ Client Build Date April  3 2014.";

            PatchConfFileName = "patch_Apr03-2014.conf";
            ExpectedPPLength = 26544; 
            SupportsSQLGeneration = false;
        }

        public override void RegisterExplorers()
        {
            OpManager.RegisterExplorer("OP_PlayerProfile", ExplorePlayerProfile);
            OpManager.RegisterExplorer("OP_ZoneEntry", ExploreZoneEntry);
        }

        public override IdentificationStatus Identify(int opCode, int size, PacketDirection direction)
        {
            if ((opCode == OpManager.OpCodeNameToNumber("OP_ZoneEntry")) &&
                (direction == PacketDirection.ClientToServer))
                return IdentificationStatus.Tentative;

            if ((opCode == OpManager.OpCodeNameToNumber("OP_PlayerProfile")) &&
                direction == PacketDirection.ServerToClient) // &&(Size == ExpectedPPLength))
                return IdentificationStatus.Yes;


            return IdentificationStatus.No;
        }

        //See SpawnShell.cpp int32_t SpawnShell::fillSpawnStruct(spawnStruct *spawn, const uint8_t *data, size_t len, bool checkLen)?
        public override void ExploreZoneEntry(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            if (direction != PacketDirection.ServerToClient)
                return;
            try
            {
                string FirstName = buffer.ReadString(false);

                outputStream.WriteLine("Name = {0}", FirstName);
                if (FirstName == "Emperor_Crush00")
                {
                    outputStream.WriteLine("Sample marker");
                }
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

                buffer.SkipBytes(12); // Drakkin stuff
                byte EquipChest2 = buffer.ReadByte();
                buffer.SkipBytes(2);
                byte Helm = buffer.ReadByte();


                float Size = buffer.ReadSingle();

                byte Face = buffer.ReadByte();

                float WalkSpeed = buffer.ReadSingle();

                float RunSpeed = buffer.ReadSingle();

                UInt32 Race = buffer.ReadUInt32();

                outputStream.WriteLine("Size: {0}, Face: {1}, Walkspeed: {2}, RunSpeed: {3}, Race: {4}", Size, Face,
                    WalkSpeed, RunSpeed, Race);

                //vsabL above here is correct, below is incorrect time to get from seq...
                outputStream.WriteLine("Holding = {0}", buffer.ReadByte());
                outputStream.WriteLine("Deity = {0}", buffer.ReadUInt32());
                outputStream.WriteLine("GuildID = {0}", buffer.ReadUInt32());
                outputStream.WriteLine("Guildstatus = {0}", buffer.ReadUInt32());
                outputStream.WriteLine("Classs? = {0}", buffer.ReadUInt32());

                buffer.SkipBytes(1);
                outputStream.WriteLine("State = {0}", buffer.ReadByte());
                outputStream.WriteLine("Light = {0}", buffer.ReadByte());
                                buffer.SkipBytes(1);

                outputStream.WriteLine("LastName = {0}", buffer.ReadString(false));
                buffer.SkipBytes(6);
                outputStream.WriteLine("PetOwnerId = {0}", buffer.ReadUInt32());
                if (IsNPC==1)
                {
                    buffer.SkipBytes(37);
                }
                else
                {
                    buffer.SkipBytes(25);
                }

                if (IsNPC == 0 || NPCType.IsPlayableRace(Race))
                {
                    //for (int ColourSlot = 0; ColourSlot < 9; ++ColourSlot)
                    //     outputStream.WriteLine("Color {0} is {1}", ColourSlot, buffer.ReadUInt32());
                    buffer.SkipBytes(36);

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

                    UInt32 MeleeTexture1 = buffer.ReadUInt32();
                    buffer.SkipBytes(16);
                    UInt32 MeleeTexture2 = buffer.ReadUInt32();
                    buffer.SkipBytes(16);
                }

                outputStream.WriteLine("Position starts at offset {0}", buffer.GetPosition());

/*
*          
union
{
    struct
    {
        unsigned pitch:12;
        signed   deltaX:13;                       // change in x
        unsigned padding01:7;
        signed   z:19;                            // z coord (3rd loc value)
        signed   deltaHeading:10;                 // change in heading 
        unsigned padding02:3;
        signed   x:19;                            // x coord (1st loc value)
        signed   deltaZ:13;                       // change in z
        unsigned heading:12;                      // heading 
        signed   deltaY:13;                       // change in y
        unsigned padding03:7;
        signed   animation:10;                    // velocity 
        signed   y:19;                            // y coord (2nd loc value)
        unsigned padding04:3;
    };
    int32_t posData[5];
};*/

                UInt32 Position1 = buffer.ReadUInt32();
                outputStream.WriteLine("Position1 untreated {0}", Position1);
                UInt32 Position2 = buffer.ReadUInt32();
                outputStream.WriteLine("Position2 untreated {0}", Position2);
                UInt32 Position3 = buffer.ReadUInt32();
                outputStream.WriteLine("Position3 untreated {0}", Position3); //verified as X position
                UInt32 Position4 = buffer.ReadUInt32();
                outputStream.WriteLine("Position4 untreated {0}", Position4);

                UInt32 Position5 = buffer.ReadUInt32(); //verified as Y position

                float XPos = Utils.EQ19ToFloat((Int32)(Position3) & 0x7FFFF);  //Verified
                float YPos = Utils.EQ19ToFloat((Int32) (Position5 >> 10) & 0x7FFFF); //Verified
                float ZPos = Utils.EQ19ToFloat((Int32) ((Position2) & 0x7FFFF)); //thanks Demonstar55
                //heading is definitely NOT Position3
                float Heading = Utils.EQ19ToFloat((Int32)(Position4) & 0xFFF); //can't verify

                //for(var i = 0; i < 32; ++i)
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

                outputStream.WriteLine("Buffer Length: {0}, Current Position: {1}", buffer.Length(),
                    buffer.GetPosition());

                if (buffer.Length() != buffer.GetPosition())
                    outputStream.WriteLine("PARSE ERROR");

                outputStream.WriteLine("");
            }
            catch (Exception)
            {
            }
        }

        /*
         * from seq source int32_t ZoneMgr::fillProfileStruct(charProfileStruct *player, const uint8_t *data, size_t len, bool checkLen)
         * 
         * */

        public override void ExplorePlayerProfile(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            outputStream.WriteLine("MooDump");
            outputStream.WriteLine("{0, -5}: Checksum = {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            // Unknown  
            buffer.SkipBytes(12);
            outputStream.WriteLine("");
            outputStream.WriteLine("{0, -5}: Gender = {1}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Race = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Class = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Level = {1}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Level1 = {1}", buffer.GetPosition(), buffer.ReadByte());

            // Bind points
            int BindCount = buffer.ReadInt32();

            outputStream.WriteLine("{0, -5}: BindCount = {1}", buffer.GetPosition() - 4, BindCount);
            //not sure if this is right tbh
            for (int i = 0; i < BindCount; ++i)
            {
                outputStream.WriteLine("{0, -5}:   Bind: {1} Zone: {2} XYZ: {3},{4},{5} Heading: {6}",
                    buffer.GetPosition(), i, buffer.ReadUInt32(), buffer.ReadSingle(), buffer.ReadSingle(),
                    buffer.ReadSingle(), buffer.ReadSingle());
            }
            outputStream.WriteLine("");
            outputStream.WriteLine("{0, -5}: Deity = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Intoxication = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("");
            // Spell slot refresh
            int spellRefreshCount = buffer.ReadInt32();
            outputStream.WriteLine("{0, -5}: SpellRefreshCount = {1}", buffer.GetPosition() - 4, spellRefreshCount);
            for (int i = 0; i < spellRefreshCount; i++)
            {
                outputStream.WriteLine("{0, -5}: SpellRefreshCount{1} = {2}", buffer.GetPosition(), i,
                    buffer.ReadUInt32());
            }

            // Equipment
            uint EquipmentCount = buffer.ReadUInt32();

            outputStream.WriteLine("{0, -5}: EquipmentCount = {1}", buffer.GetPosition() - 4, EquipmentCount);

            for (int i = 0; i < EquipmentCount; ++i)
            {
                outputStream.Write("{0, -5}: Equip: {1} Values: ", buffer.GetPosition(), i);
                for (int j = 0; j < 5; ++j)
                {
                    outputStream.Write(j != 3 ? "{0} " : " ItemId {0} ", buffer.ReadUInt32());
                }
                outputStream.WriteLine("");
            }

            // Something (9 ints)
            var sCount = buffer.ReadUInt32();
            for (var i = 0; i < sCount; i++)
            {
                buffer.SkipBytes(20);
            }

            // Something (9 ints)
            var sCount1 = buffer.ReadUInt32();
            for (var i = 0; i < sCount1; i++)
            {
                buffer.SkipBytes(4);
            }

            // Something (9 ints)
            var sCount2 = buffer.ReadUInt32();
            for (var i = 0; i < sCount2; i++)
            {
                buffer.SkipBytes(4);
            }



            int preposn = buffer.GetPosition();

            outputStream.WriteLine("{0, -5}: Hair Color = {1}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Beard Color = {1}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Unknown = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Eye1 Color = {1}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Eye2 Color = {1}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Hairstyle = {1}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Beard = {1}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Face = {1}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Drakkin Heritage = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Drakkin Tattoo = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Drakkin Details = {1}", buffer.GetPosition(), buffer.ReadUInt32());

            outputStream.WriteLine("{0, -5}: Unknown = {1}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Unknown = {1}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Unknown = {1}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Unknown = {1}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Unknown = {1}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Height = {1}", buffer.GetPosition(), buffer.ReadSingle());
            outputStream.WriteLine("{0, -5}: Unknown = {1}", buffer.GetPosition(), buffer.ReadSingle());
            outputStream.WriteLine("{0, -5}: Unknown = {1}", buffer.GetPosition(), buffer.ReadSingle());
            outputStream.WriteLine("{0, -5}: Unknown = {1}", buffer.GetPosition(), buffer.ReadSingle());
            outputStream.WriteLine("{0, -5}: Primary = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Secondary = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            int diff = buffer.GetPosition() - preposn;
            outputStream.WriteLine("Diff should be 52: {0}", diff);
            // Looks like face, haircolor, beardcolor, eyes, etc. Skipping over it.
            //Buffer.SkipBytes(52);
            outputStream.WriteLine("{0, -5}: Unspent Skill Points = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Mana = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Current HP = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: STR = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: STA = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: CHA = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: DEX = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: INT = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: AGI = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: WIS = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            buffer.SkipBytes(28);
            //Buffer.SkipBytes(28);

            UInt32 AACount = buffer.ReadUInt32();

            outputStream.WriteLine("{0, -5}: AA Count = {1}", buffer.GetPosition() - 4, AACount);
            for (int i = 0; i < AACount; ++i)
            {
                outputStream.WriteLine("   AA: {0}, Value: {1}, Unknown08: {2}", buffer.ReadUInt32(),
                    buffer.ReadUInt32(), buffer.ReadUInt32());
                //Buffer.SkipBytes(12);
            }

            // Something (100 ints)
            uint sCount3 = buffer.ReadUInt32();
            for (int i = 0; i < sCount3; i++)
            {
                buffer.SkipBytes(4);
            }

            // Something (25 ints)
            uint sCount4 = buffer.ReadUInt32();
            for (int i = 0; i < sCount4; i++)
            {
                buffer.SkipBytes(4);
            }

            // Something (300 ints)
            uint sCount5 = buffer.ReadUInt32();
            for (int i = 0; i < sCount5; i++)
            {
                buffer.SkipBytes(4);
            }

            // Something (20 ints)
            uint sCount6 = buffer.ReadUInt32();
            for (int i = 0; i < sCount6; i++)
            {
                buffer.SkipBytes(4);
            }

            // Something (20 floats)
            uint sCount7 = buffer.ReadUInt32();
            for (int i = 0; i < sCount7; i++)
            {
                buffer.SkipBytes(4);
            }

            // Something (100 floats)
            uint sCount8 = buffer.ReadUInt32();
            for (int i = 0; i < sCount8; i++)
            {
                buffer.SkipBytes(4);
            }


            UInt32 SpellBookSlots = buffer.ReadUInt32();

            outputStream.WriteLine("{0, -5}: SpellBookSlot Count = {1}", buffer.GetPosition() - 4, SpellBookSlots);

            for (int i = 0; i < SpellBookSlots; ++i)
            {
                buffer.SkipBytes(4);
            }

            UInt32 SpellMemSlots = buffer.ReadUInt32();

            outputStream.WriteLine("{0, -5}: Spell Mem Count = {1}", buffer.GetPosition() - 4, SpellMemSlots);

            for (int i = 0; i < SpellMemSlots; ++i)
            {
                buffer.SkipBytes(4);
            }

            // Something (13 ints)
            uint sCount9 = buffer.ReadUInt32();
            for (int i = 0; i < sCount9; i++)
            {
                buffer.SkipBytes(4);
            }

            // Unknown
            buffer.SkipBytes(1);
            UInt32 BuffCount = buffer.ReadUInt32();

            outputStream.WriteLine("{0, -5}: Buff Count = {1}", buffer.GetPosition() - 4, BuffCount);

            for (int i = 0; i < BuffCount; ++i)
            {
                buffer.ReadByte();
                float UnkFloat = buffer.ReadSingle();
                UInt32 PlayerID = buffer.ReadUInt32();
                Byte UnkByte = buffer.ReadByte();
                UInt32 Counters1 = buffer.ReadUInt32();
                UInt32 Duration = buffer.ReadUInt32();
                Byte Level = buffer.ReadByte();
                UInt32 SpellID = buffer.ReadUInt32();
                UInt32 SlotID = buffer.ReadUInt32();
                buffer.SkipBytes(5);
                UInt32 Counters2 = buffer.ReadUInt32();
                outputStream.WriteLine(
                    "Sl: {0}, UF: {1}, PID: {2}, UByte: {3}, Cnt1: {4}, Dur: {5}, Lvl: {6} SpellID: {7}, SlotID: {8}, Cnt2: {9}",
                    i, UnkFloat, PlayerID, UnkByte, Counters1, Duration, Level, SpellID, SlotID, Counters2);
                buffer.SkipBytes(44);
            }

            outputStream.WriteLine("{0, -5}: Plat = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Gold = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Silver = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Copper = {1}", buffer.GetPosition(), buffer.ReadUInt32());

            outputStream.WriteLine("{0, -5}: Plat Cursor = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Gold Cursor = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Silver Cursor = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Copper Cursor = {1}", buffer.GetPosition(), buffer.ReadUInt32());

            // Unknown
            buffer.SkipBytes(20);

            outputStream.WriteLine("{0, -5}: AA Spent = {1}", buffer.GetPosition(), buffer.ReadUInt32());

            // Unknown
            buffer.SkipBytes(4);


            outputStream.WriteLine("{0, -5}: AA Assigned = {1}", buffer.GetPosition(), buffer.ReadUInt32());

            // Unknown
            buffer.SkipBytes(16);

            outputStream.WriteLine("{0, -5}: AA Unspent = {1}", buffer.GetPosition(), buffer.ReadUInt32());

            // Unknown
            buffer.SkipBytes(2);
/*
  // Bandolier
  Buffer.SkipBytes(1319);

  // Potion Belt
  Buffer.SkipBytes(160);
*/
            //this could be wrong we could just skip
            UInt32 BandolierCount = buffer.ReadUInt32();

            outputStream.WriteLine("{0, -5}: Bandolier Count = {1}", buffer.GetPosition() - 4, BandolierCount);

            for (int i = 0; i < BandolierCount; ++i)
            {
                buffer.ReadString(false);

                buffer.ReadString(false);
                buffer.SkipBytes(8);

                buffer.ReadString(false);
                buffer.SkipBytes(8);

                buffer.ReadString(false);
                buffer.SkipBytes(8);

                buffer.ReadString(false);
                buffer.SkipBytes(8);
            }

            UInt32 PotionCount = buffer.ReadUInt32();

            outputStream.WriteLine("{0, -5}: Potion Count = {1}", buffer.GetPosition() - 4, PotionCount);

            for (int i = 0; i < PotionCount; ++i)
            {
                buffer.ReadString(false);
                buffer.SkipBytes(8);
            }

            // Unknown
            buffer.SkipBytes(84);

            outputStream.WriteLine("{0, -5}: Endurance= {1}", buffer.GetPosition(), buffer.ReadUInt32());

            // Unknown
            buffer.SkipBytes(8);

            
            UInt32 NameLength = buffer.ReadUInt32();
            var posn = buffer.GetPosition();
            outputStream.WriteLine("{0, -5}: Name Length: {1}", buffer.GetPosition() - 4, NameLength);
            var name = buffer.ReadString(false);
            outputStream.WriteLine("{0, -5}: Name: {1}", buffer.GetPosition(), name);
     
            int CurrentPosition = buffer.GetPosition();
            diff = CurrentPosition - posn;
            var skip = (int) NameLength - diff;
            outputStream.WriteLine("Diff is {0}. If it is not 0, then we will go overboard when setting posn. Skipping {1} bytes", diff, skip);
            buffer.SkipBytes(skip);

            UInt32 LastNameLength = buffer.ReadUInt32();
            posn = buffer.GetPosition();
            outputStream.WriteLine("{0, -5}: LastName Length: {1}", buffer.GetPosition() - 4, LastNameLength);

            name = buffer.ReadString(false);
            outputStream.WriteLine("{0, -5}: Last Name: {1}", buffer.GetPosition(), name);
            CurrentPosition = buffer.GetPosition();
            diff = CurrentPosition - posn;
            skip = (int)LastNameLength - diff;
            outputStream.WriteLine("Diff is {0}. If it is not 0, then we will go overboard when setting posn. Skipping {1} bytes", diff, skip);
            buffer.SkipBytes(skip);

            // Unknown
            //Buffer.SkipBytes(4);
            outputStream.WriteLine("{0, -5}: Birthday {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Account start date {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Last Save Date {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Time played in Minutes {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Time Entitled On Account {1}", buffer.GetPosition(),buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Expansions {1:X}", buffer.GetPosition(), buffer.ReadUInt32());

            UInt32 LanguageCount = buffer.ReadUInt32();

            outputStream.WriteLine("{0, -5}: Language Count = {1}", buffer.GetPosition() - 4, LanguageCount);

            for (int i = 0; i < LanguageCount; ++i)
            {
                buffer.SkipBytes(1);
            }

            outputStream.WriteLine("{0, -5}: Zone ID {1}", buffer.GetPosition(), buffer.ReadUInt16());
            outputStream.WriteLine("{0, -5}: Zone Instance {1}", buffer.GetPosition(), buffer.ReadUInt16());
            outputStream.WriteLine("{0, -5}: Y,X,Z {1},{2},{3} Heading: {4}",
                buffer.GetPosition(), buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle(),
                buffer.ReadSingle());

            //vsab all above here is verified
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadByte());

            outputStream.WriteLine("{0, -5}: GuildID? {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());

            outputStream.WriteLine("{0, -5}: Experience {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadByte());

            outputStream.WriteLine("{0, -5}: Bank Plat {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Bank Gold {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Bank Silver {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Bank Copper {1}", buffer.GetPosition(), buffer.ReadUInt32());

            outputStream.WriteLine("{0, -5}: Shared plat? {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());

            UInt32 Unknown42 = buffer.ReadUInt32();
            outputStream.WriteLine("{0, -5}: Unknown, value 42? {1}", buffer.GetPosition() - 4, Unknown42);

            buffer.SkipBytes((int)(Unknown42 * 8));

            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());

            outputStream.WriteLine("{0, -5}: Career Tribute Favour {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Current Tribute Favour {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());

            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadByte());

            UInt32 PersonalTributeCount = buffer.ReadUInt32();
            outputStream.WriteLine("{0, -5}: Personal Tribute Count {1}", buffer.GetPosition() - 4,
                PersonalTributeCount);
            buffer.SkipBytes((int)(PersonalTributeCount * 8));

            UInt32 GuildTributeCount = buffer.ReadUInt32();
            outputStream.WriteLine("{0, -5}: Guild Tribute Count {1}", buffer.GetPosition() - 4, GuildTributeCount);
            buffer.SkipBytes((int)(GuildTributeCount * 8));

            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());

            outputStream.WriteLine("Skipping 121 bytes starting at offset {0}", buffer.GetPosition());
            buffer.SkipBytes(121);

            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());

            outputStream.WriteLine("Position now {0}", buffer.GetPosition());

            UInt32 Unknown64 = buffer.ReadUInt32();
            outputStream.WriteLine("{0, -5}: Unknown64 {1}", buffer.GetPosition() - 4, Unknown64);
            buffer.SkipBytes((int)Unknown64);

            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadByte());

            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());

            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadByte());

            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());

            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadByte());

            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());

            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadByte());

            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());

            Unknown64 = buffer.ReadUInt32();
            outputStream.WriteLine("{0, -5}: Unknown64 {1}", buffer.GetPosition() - 4, Unknown64);
            buffer.SkipBytes((int)Unknown64);

            Unknown64 = buffer.ReadUInt32();
            outputStream.WriteLine("{0, -5}: Unknown64 {1}", buffer.GetPosition() - 4, Unknown64);
            buffer.SkipBytes((int)Unknown64);

            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());

            outputStream.WriteLine("Skipping 320 bytes starting at offset {0}", buffer.GetPosition());
            buffer.SkipBytes(320);

            outputStream.WriteLine("Skipping 343 bytes starting at offset {0}", buffer.GetPosition());
            buffer.SkipBytes(343);

            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());

            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadByte());

            UInt32 Unknown6 = buffer.ReadUInt32();
            outputStream.WriteLine("{0, -5}: Unknown6 {1} LDON Stuff then why have the count before it?", buffer.GetPosition() - 4, Unknown6);

            //for (int i = 0; i < Unknown6; ++i)
            //{
            //    OutputStream.WriteLine("{0, -5}: Unknown LDON? {1}", Buffer.GetPosition(), Buffer.ReadUInt32());
            //}
            outputStream.WriteLine("{0, -5}: Ldon GUK points? {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Ldon MIR points? {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Ldon mmc points? {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Ldon ruj points? {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Ldon tak points? {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Ldon available points? {1}", buffer.GetPosition(), buffer.ReadUInt32());


            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());

            Unknown64 = buffer.ReadUInt32();
            outputStream.WriteLine("{0, -5}: Unknown64 {1}", buffer.GetPosition() - 4, Unknown64);
            buffer.SkipBytes((int)Unknown64 * 4);

            // Air remaining ?
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());

            // Next 7 could be PVP stats,
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());

            // PVP LastKill struct ?
            outputStream.WriteLine("Skipping string + 24 bytes starting at offset {0}", buffer.GetPosition());
            //Buffer.SkipBytes(25);

            Byte b;
            do
            {
                b = buffer.ReadByte();
            } while (b != 0);

            buffer.SkipBytes(24);

            // PVP LastDeath struct ?
            outputStream.WriteLine("Skipping string + 24 bytes starting at offset {0}", buffer.GetPosition());
            //Buffer.SkipBytes(25);
            do
            {
                b = buffer.ReadByte();
            } while (b != 0);

            buffer.SkipBytes(24);

            // PVP Number of Kills in Last 24 hours ?
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());

            UInt32 Unknown50 = buffer.ReadUInt32();
            outputStream.WriteLine("{0, -5}: Unknown50 {1}", buffer.GetPosition() - 4, Unknown50);
            // PVP Recent Kills ?
            outputStream.WriteLine("Pss pvp recent kills Skipping 50 x (String + 24 bytes) starting at offset {0}", buffer.GetPosition());
            //Buffer.SkipBytes(1338);
            for (int i = 0; i < 50; ++i)
            {
                do
                {
                    b = buffer.ReadByte();
                } while (b != 0);

                buffer.SkipBytes(24);
            }


            //

            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());

            outputStream.WriteLine("{0, -5}: Group autoconsent? {1:X}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Raid autoconsent? {1:X}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Guild autoconsent? {1:X}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadByte());

            outputStream.WriteLine("{0, -5}: Level3? {1}", buffer.GetPosition(), buffer.ReadUInt32());

            outputStream.WriteLine("{0, -5}: Showhelm? {1}", buffer.GetPosition(), buffer.ReadByte());

            outputStream.WriteLine("{0, -5}: RestTimer? {1}", buffer.GetPosition(), buffer.ReadUInt32());

            outputStream.WriteLine("Skipping 1028 bytes starting at offset {0}", buffer.GetPosition());
            buffer.SkipBytes(1028);

            //outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            //outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());

            outputStream.WriteLine("Pointer is {0} bytes from end.", buffer.Length() - buffer.GetPosition());
        }


        public override UInt16 GetZoneNumber()
        {
            try
            {
                List<byte[]> NewZonePacket = GetPacketsOfType("OP_NewZone", PacketDirection.ServerToClient);
                if (NewZonePacket.Count == 0)
                {
                    return 0;
                }

                var Buffer = new ByteStream(NewZonePacket[0]);
                Buffer.SkipBytes(852);
                UInt16 ZoneID = Buffer.ReadUInt16();
                return ZoneID;

            }
            catch (Exception)
            {
                return 0;
            }
        }

        override public bool DumpAAs(string fileName)
        {
            List<byte[]> AAPackets = GetPacketsOfType("OP_SendAATable", PacketDirection.ServerToClient);


            if (AAPackets.Count < 1)
                return false;

            StreamWriter OutputFile;

            try
            {
                OutputFile = new StreamWriter(fileName);
            }
            catch
            {
                return false;
            }

            OutputFile.WriteLine("-- There are " + AAPackets.Count + " OP_SendAATable packets.");
            OutputFile.WriteLine("");

            foreach (byte[] Packet in AAPackets)
            {
                ByteStream Buffer = new ByteStream(Packet);
                List<uint> prereqskills = new List<uint>();
                List<uint> prereqpoints = new List<uint>();

                UInt32 AAID = Buffer.ReadUInt32();
                byte Unknown004 = Buffer.ReadByte();
                UInt32 HotKeySID = Buffer.ReadUInt32();
                UInt32 HotKeySID2 = Buffer.ReadUInt32();
                UInt32 TitleSID = Buffer.ReadUInt32();
                UInt32 DescSID = Buffer.ReadUInt32();
                UInt32 ClassType = Buffer.ReadUInt32();
                UInt32 Cost = Buffer.ReadUInt32();
                UInt32 Seq = Buffer.ReadUInt32();
                UInt32 CurrentLevel = Buffer.ReadUInt32();

                UInt32 TotalPreReqSkills = Buffer.ReadUInt32();
                for (int i = 0; i < TotalPreReqSkills; i++)
                    prereqskills.Add(Buffer.ReadUInt32());

                UInt32 TotalPreReqPoints = Buffer.ReadUInt32();
                for (int i = 0; i < TotalPreReqPoints; i++)
                    prereqpoints.Add(Buffer.ReadUInt32());

                UInt32 Type = Buffer.ReadUInt32();
                UInt32 SpellID = Buffer.ReadUInt32();
                UInt32 Unknown057 = Buffer.ReadUInt32();
                UInt32 SpellType = Buffer.ReadUInt32();
                UInt32 SpellRefresh = Buffer.ReadUInt32();
                UInt16 Classes = Buffer.ReadUInt16();
                UInt16 Berserker = Buffer.ReadUInt16();
                UInt32 MaxLevel = Buffer.ReadUInt32();
                UInt32 LastID = Buffer.ReadUInt32();
                UInt32 NextID = Buffer.ReadUInt32();
                UInt32 Cost2 = Buffer.ReadUInt32();
                Buffer.SkipBytes(7);
                UInt32 AAExpansion = Buffer.ReadUInt32();
                UInt32 SpecialCategory = Buffer.ReadUInt32();
                Buffer.SkipBytes(8);
                UInt32 TotalAbilities = Buffer.ReadUInt32();

                OutputFile.WriteLine(String.Format("AAID: {0}", AAID));
                OutputFile.WriteLine(" Unknown004:\t" + Unknown004);
                OutputFile.WriteLine(" HotkeySID:\t" + HotKeySID);
                OutputFile.WriteLine(" HotkeySID2:\t" + HotKeySID2);
                OutputFile.WriteLine(" TitleSID:\t" + TitleSID);
                OutputFile.WriteLine(" DescSID:\t" + DescSID);
                OutputFile.WriteLine(" ClassType:\t" + ClassType);
                OutputFile.WriteLine(" Cost:\t\t" + Cost);
                OutputFile.WriteLine(" Seq:\t\t" + Seq);
                OutputFile.WriteLine(" CurrentLevel:\t" + CurrentLevel);
                OutputFile.Write(" PreReqSkill (SEQs):\t");
                foreach (var pskill in prereqskills)
                    OutputFile.Write("{0} ", pskill);

                OutputFile.WriteLine("");

                OutputFile.Write(" PreReqSkills MinPoints:\t");
                foreach (var ppoint in prereqpoints)
                    OutputFile.Write("{0} ", ppoint);

                OutputFile.WriteLine("");

                OutputFile.WriteLine(" Type:\t\t" + Type);
                OutputFile.WriteLine(" SpellID:\t" + SpellID);
                OutputFile.WriteLine(" Unknown057:\t" + Unknown057);
                OutputFile.WriteLine(" SpellType:\t" + SpellType);
                OutputFile.WriteLine(" SpellRefresh:\t" + SpellRefresh);
                OutputFile.WriteLine(" Classes:\t" + Classes);
                OutputFile.WriteLine(" Berserker:\t" + Berserker);
                OutputFile.WriteLine(" MaxLevel:\t" + MaxLevel);
                OutputFile.WriteLine(" LastID:\t" + LastID);
                OutputFile.WriteLine(" NextID:\t" + NextID);
                OutputFile.WriteLine(" Cost2:\t\t" + Cost2);
                OutputFile.WriteLine(" AAExpansion:\t" + AAExpansion);
                OutputFile.WriteLine(" SpecialCat:\t" + SpecialCategory);
                OutputFile.WriteLine("");
                OutputFile.WriteLine(" TotalAbilities:\t" + TotalAbilities);
                OutputFile.WriteLine("");

                for (int i = 0; i < TotalAbilities; ++i)
                {
                    UInt32 Ability = Buffer.ReadUInt32();
                    Int32 Base1 = Buffer.ReadInt32();
                    Int32 Base2 = Buffer.ReadInt32();
                    UInt32 Slot = Buffer.ReadUInt32();

                    OutputFile.WriteLine(String.Format("    Ability:\t{0}\tBase1:\t{1}\tBase2:\t{2}\tSlot:\t{3}", Ability, Base1, Base2, Slot));

                }
                OutputFile.WriteLine("");
                OutputFile.Flush();
            }

            OutputFile.Close();

            return true;
        }
    }
}
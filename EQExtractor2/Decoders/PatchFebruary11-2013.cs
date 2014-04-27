using System;
using System.IO;
using EQExtractor2.Domain;

namespace EQExtractor2.Decoders
{
    class PatchFebruary112013Decoder : PatchJanuary162013Decoder
    {
        public PatchFebruary112013Decoder()
        {
            Version = "EQ Client Build Date February 11 2013.";

            PatchConfFileName = "patch_Feb11-2013.conf";

            PacketsToMatch = new PacketToMatch[] {
                new PacketToMatch { OPCodeName = "OP_ZoneEntry", Direction = PacketDirection.ClientToServer, RequiredSize = 76, VersionMatched = false },
                new PacketToMatch { OPCodeName = "OP_PlayerProfile", Direction = PacketDirection.ServerToClient, RequiredSize = -1, VersionMatched = true },
            };

            SupportsSQLGeneration = false;
        }

        public override void RegisterExplorers()
        {
            //OpManager.RegisterExplorer("OP_PlayerProfile", ExplorePlayerProfile);
            //OpManager.RegisterExplorer("OP_CharInventory", ExploreInventory);
        }

        public override void DecodeItemPacket(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            String UnkString = buffer.ReadString(false);
            //Buffer.SkipBytes(88);
            buffer.SkipBytes(35);
            UInt32 RecastTimer = buffer.ReadUInt32();
            buffer.SkipBytes(49);
            String ItemName = buffer.ReadString(false);
            String ItemLore = buffer.ReadString(false);
            String ItemIDFile = buffer.ReadString(false);
            buffer.ReadString(false);

            UInt32 ItemID = buffer.ReadUInt32();
            outputStream.WriteLine("ItemName: {0}, IDFile: {1}", ItemName, ItemIDFile);
            outputStream.WriteLine("Recast Time: {0:X}", RecastTimer);
            buffer.SkipBytes(251);

            String CharmFile = buffer.ReadString(false);

            outputStream.WriteLine("CharmFile: {0}", CharmFile);

            buffer.SkipBytes(74);   // Secondary BS

            String FileName = buffer.ReadString(false);
            outputStream.WriteLine("FileName: {0}", CharmFile);

            buffer.SkipBytes(76);   // Tertiary BS

            UInt32 ClickEffect = buffer.ReadUInt32();
            //Buffer.SkipBytes(26);   // ClickEffect Struct
            //OutputStream.WriteLine("Click Effect - effect : {0}", Buffer.ReadUInt32());
            outputStream.WriteLine("Click Effect - level2 : {0}", buffer.ReadByte());
            outputStream.WriteLine("Click Effect - Type : {0}", buffer.ReadUInt32());
            outputStream.WriteLine("Click Effect - level : {0}", buffer.ReadByte());
            outputStream.WriteLine("Click Effect - Max Charges : {0}", buffer.ReadUInt32());
            outputStream.WriteLine("Click Effect - Cast Time : {0}", buffer.ReadUInt32());
            outputStream.WriteLine("Click Effect - Recast : {0}", buffer.ReadUInt32());
            outputStream.WriteLine("Click Effect - Recast Type: {0}", buffer.ReadUInt32());
            outputStream.WriteLine("Click Effect - Unk5: {0}", buffer.ReadUInt32());
            String ClickName = buffer.ReadString(false);
            outputStream.WriteLine("ClickEffect = {0}, ClickName = {1}", ClickEffect, ClickName);
            buffer.ReadUInt32();

            ClickEffect = buffer.ReadUInt32();
            buffer.SkipBytes(26);   // ClickEffect Struct
            ClickName = buffer.ReadString(false);
            outputStream.WriteLine("ClickEffect = {0}, ClickName = {1}", ClickEffect, ClickName);
            buffer.ReadUInt32();

            ClickEffect = buffer.ReadUInt32();
            buffer.SkipBytes(26);   // ClickEffect Struct
            ClickName = buffer.ReadString(false);
            outputStream.WriteLine("ClickEffect = {0}, ClickName = {1}", ClickEffect, ClickName);
            buffer.ReadUInt32();

            ClickEffect = buffer.ReadUInt32();
            buffer.SkipBytes(26);   // ClickEffect Struct
            ClickName = buffer.ReadString(false);
            outputStream.WriteLine("ClickEffect = {0}, ClickName = {1}", ClickEffect, ClickName);
            buffer.ReadUInt32();

            ClickEffect = buffer.ReadUInt32();
            buffer.SkipBytes(26);   // ClickEffect Struct
            ClickName = buffer.ReadString(false);
            outputStream.WriteLine("ClickEffect = {0}, ClickName = {1}", ClickEffect, ClickName);
            buffer.ReadUInt32();

            ClickEffect = buffer.ReadUInt32();
            buffer.SkipBytes(26);   // ClickEffect Struct
            ClickName = buffer.ReadString(false);
            outputStream.WriteLine("ClickEffect = {0}, ClickName = {1}", ClickEffect, ClickName);
            buffer.ReadUInt32();

            //Buffer.SkipBytes(167);
            buffer.SkipBytes(125);
            //Byte UnkByte = Buffer.ReadByte();
            //OutputStream.WriteLine("Unk byte is {0:X}", UnkByte);
            outputStream.WriteLine("At String ? Pos is {0}", buffer.GetPosition());
            UnkString = buffer.ReadString(false);
            outputStream.WriteLine("Unk String is {0}", UnkString);
            buffer.SkipBytes(41);
            UInt32 SubItemCount = buffer.ReadUInt32();

            outputStream.WriteLine("Buffer Pos: {0}, SubItemCount = {1}", buffer.GetPosition(), SubItemCount);

            for (int j = 0; j < SubItemCount; ++j)
            {
                buffer.ReadUInt32();
                DecodeItemPacket(outputStream, buffer, direction);
            }


        }


        public override void ExploreInventory(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            if (buffer.Length() < 4)
                return;

            UInt32 Count = buffer.ReadUInt32();

            for (int i = 0; i < Count; ++i)
            {
                try
                {
                    DecodeItemPacket(outputStream, buffer, direction);
                }
                catch
                {
                    return;
                }
            }
        }
        public override void ExplorePlayerProfile(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            outputStream.WriteLine("{0, -5}: Checksum = {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: ChecksumSize = {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown = {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown = {1:X}", buffer.GetPosition(), buffer.ReadUInt32());

            outputStream.WriteLine("");
            outputStream.WriteLine("{0, -5}: Gender = {1}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Race = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Class = {1}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Level = {1}", buffer.GetPosition(), buffer.ReadByte());
            outputStream.WriteLine("{0, -5}: Level1 = {1}", buffer.GetPosition(), buffer.ReadByte());

            outputStream.WriteLine("");
            UInt32 BindCount = buffer.ReadUInt32();
            outputStream.WriteLine("{0, -5}: BindCount = {1}", buffer.GetPosition() - 4, BindCount);

            for (int i = 0; i < BindCount; ++i)
            {
                outputStream.WriteLine("{0, -5}:   Bind: {1} Zone: {2} XYZ: {3},{4},{5} Heading: {6}",
                    buffer.GetPosition(), i, buffer.ReadUInt32(), buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle());
            }

            outputStream.WriteLine("");
            outputStream.WriteLine("{0, -5}: Deity = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Intoxication = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("");

            //Buffer.SkipBytes(8); // Deity, intoxication

            UInt32 UnknownCount = buffer.ReadUInt32();

            outputStream.WriteLine("{0, -5}: Unknown Count = {1}", buffer.GetPosition() - 4, UnknownCount);



            for (int i = 0; i < UnknownCount; ++i)
            {
                outputStream.WriteLine("{0, -5}: Unknown : {1}, Value = {2}", buffer.GetPosition(), i, buffer.ReadUInt32());
                //Buffer.SkipBytes(4);
            }

            UInt32 EquipmentCount = buffer.ReadUInt32();

            outputStream.WriteLine("{0, -5}: EquipmentCount = {1}", buffer.GetPosition() - 4, EquipmentCount);

            for (int i = 0; i < EquipmentCount; ++i)
            {
                outputStream.Write("{0, -5}: Equip: {1} Values: ", buffer.GetPosition(), i);
                for (int j = 0; j < 5; ++j)
                    outputStream.Write("{0} ", buffer.ReadUInt32());

                outputStream.WriteLine("");
                //Buffer.SkipBytes(20);
            }

            UInt32 EquipmentCount2 = buffer.ReadUInt32();

            outputStream.WriteLine("{0, -5}: EquipmentCount2 = {1}", buffer.GetPosition() - 4, EquipmentCount2);

            for (int i = 0; i < EquipmentCount2; ++i)
            {
                outputStream.Write("{0, -5}: Equip2: {1} Values: ", buffer.GetPosition(), i);
                for (int j = 0; j < 5; ++j)
                    outputStream.Write("{0} ", buffer.ReadUInt32());

                outputStream.WriteLine("");
                //Buffer.SkipBytes(20);
            }



            UInt32 TintCount = buffer.ReadUInt32();

            outputStream.WriteLine("{0, -5}: TintCount = {1}", buffer.GetPosition() - 4, TintCount);

            for (int i = 0; i < TintCount; ++i)
            {
                outputStream.WriteLine("{0, -5}: TintCount : {1}, Value = {2}", buffer.GetPosition(), i, buffer.ReadUInt32());
                //Buffer.SkipBytes(4);
            }

            UInt32 TintCount2 = buffer.ReadUInt32();

            outputStream.WriteLine("{0, -5}: TintCount2 = {1}", buffer.GetPosition() - 4, TintCount2);

            for (int i = 0; i < TintCount; ++i)
            {
                outputStream.WriteLine("{0, -5}: TintCount2 : {1}, Value = {2}", buffer.GetPosition(), i, buffer.ReadUInt32());
                //Buffer.SkipBytes(4);
            }

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



            //Buffer.SkipBytes(52);   // Per SEQ, this looks like face, haircolor, beardcolor etc.
            outputStream.WriteLine("{0, -5}: Unspent Skill Points = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Mana = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Current HP = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            //UInt32 Points = Buffer.ReadUInt32();
            //UInt32 Mana = Buffer.ReadUInt32();
            //UInt32 CurHP = Buffer.ReadUInt32();

            //OutputStream.WriteLine("Points, Mana, CurHP = {0}, {1}, {2}", Points, Mana, CurHP);

            outputStream.WriteLine("{0, -5}: STR = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: STA = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: CHA = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: DEX = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: INT = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: AGI = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: WIS = {1}", buffer.GetPosition(), buffer.ReadUInt32());

            outputStream.WriteLine("{0, -5}: Unknown = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown = {1}", buffer.GetPosition(), buffer.ReadUInt32());

            //Buffer.SkipBytes(28);
            //Buffer.SkipBytes(28);

            UInt32 AACount = buffer.ReadUInt32();

            outputStream.WriteLine("{0, -5}: AA Count = {1}", buffer.GetPosition() - 4, AACount);


            for (int i = 0; i < AACount; ++i)
            {
                outputStream.WriteLine("   AA: {0}, Value: {1}, Unknown08: {2}", buffer.ReadUInt32(), buffer.ReadUInt32(), buffer.ReadUInt32());
                //Buffer.SkipBytes(12);
            }

            UInt32 SkillCount = buffer.ReadUInt32();

            outputStream.WriteLine("{0, -5}: Skill Count = {1}", buffer.GetPosition() - 4, SkillCount);

            for (int i = 0; i < SkillCount; ++i)
            {
                buffer.SkipBytes(4);
            }

            UInt32 SomethingCount = buffer.ReadUInt32();

            outputStream.WriteLine("{0, -5}: Something Count = {1}", buffer.GetPosition() - 4, SomethingCount);


            for (int i = 0; i < SomethingCount; ++i)
            {
                //Buffer.SkipBytes(4);
                outputStream.WriteLine("Something {0} : {1}", i, buffer.ReadUInt32());
            }

            UInt32 DisciplineCount = buffer.ReadUInt32();
            outputStream.WriteLine("{0, -5}: Discipline Count = {1}", buffer.GetPosition() - 4, DisciplineCount);

            for (int i = 0; i < DisciplineCount; ++i)
            {
                buffer.SkipBytes(4);
            }

            UInt32 TimeStampCount = buffer.ReadUInt32();
            outputStream.WriteLine("{0, -5}: TimeStamp Count = {1}", buffer.GetPosition() - 4, TimeStampCount);

            for (int i = 0; i < TimeStampCount; ++i)
            {
                //Buffer.SkipBytes(4);
                outputStream.WriteLine("Timestamp {0} : {1}", i, buffer.ReadUInt32());
            }

            System.DateTime dateTime;
            UInt32 RecastCount = buffer.ReadUInt32();

            outputStream.WriteLine("{0, -5}: Recast Count = {1}", buffer.GetPosition() - 4, RecastCount);

            for (int i = 0; i < RecastCount; ++i)
            {
                //Buffer.SkipBytes(4);
                UInt32 TimeStamp = buffer.ReadUInt32();
                dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                dateTime = dateTime.AddSeconds(TimeStamp);
                outputStream.WriteLine("Recast {0} : {1} {2}", i, TimeStamp, dateTime.ToString());
            }

            UInt32 TimeStamp2Count = buffer.ReadUInt32();
            outputStream.WriteLine("{0, -5}: TimeStamp2 Count = {1}", buffer.GetPosition() - 4, TimeStamp2Count);

            for (int i = 0; i < TimeStamp2Count; ++i)
            {
                //Buffer.SkipBytes(4);
                UInt32 TimeStamp = buffer.ReadUInt32();
                dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                dateTime = dateTime.AddSeconds(TimeStamp);

                outputStream.WriteLine("Timestamp {0} : {1} {2}", i, TimeStamp, dateTime.ToString());
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

            SomethingCount = buffer.ReadUInt32();

            outputStream.WriteLine("{0, -5}: Unknown Count = {1}", buffer.GetPosition() - 4, SomethingCount);

            for (int i = 0; i < SomethingCount; ++i)
            {
                //Buffer.SkipBytes(4);
                outputStream.WriteLine("Unknown {0} : {1}", i, buffer.ReadUInt32());

            }

            outputStream.WriteLine("{0, -5}: Unknown = {1}", buffer.GetPosition(), buffer.ReadByte());

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
                outputStream.WriteLine("Sl: {0}, UF: {1}, PID: {2}, UByte: {3}, Cnt1: {4}, Dur: {5}, Lvl: {6} SpellID: {7}, SlotID: {8}, Cnt2: {9}",
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

            outputStream.WriteLine("{0, -5}: Unknown = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Toxicity? = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Thirst? = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Hunger? = {1}", buffer.GetPosition(), buffer.ReadUInt32());

            //Buffer.SkipBytes(20);

            outputStream.WriteLine("{0, -5}: AA Spent = {1}", buffer.GetPosition(), buffer.ReadUInt32());

            outputStream.WriteLine("{0, -5}: AA Point Count? = {1}", buffer.GetPosition(), buffer.ReadUInt32());

            outputStream.WriteLine("{0, -5}: AA Assigned = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: AA Spent General = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: AA Spent Archetype = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: AA Spent Class = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: AA Spent Special = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: AA Unspent = {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown", buffer.GetPosition(), buffer.ReadUInt16());


            //Buffer.SkipBytes(30);

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

            outputStream.WriteLine("{0, -5}: Unknown {1}", buffer.GetPosition(), buffer.ReadInt32());
            outputStream.WriteLine("{0, -5}: Item HP Total? {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Endurance Total? {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Mana Total? {1}", buffer.GetPosition(), buffer.ReadUInt32());

            outputStream.WriteLine("{0, -5}: Unknown {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1}", buffer.GetPosition(), buffer.ReadUInt32());

            outputStream.WriteLine("{0, -5}: Expansion Count {1}", buffer.GetPosition(), buffer.ReadUInt32());

            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());

            UInt32 NameLength = buffer.ReadUInt32();
            outputStream.WriteLine("{0, -5}: Name Length: {1}", buffer.GetPosition() - 4, NameLength);

            int CurrentPosition = buffer.GetPosition();
            outputStream.WriteLine("{0, -5}: Name: {1}", buffer.GetPosition(), buffer.ReadString(false));

            buffer.SetPosition(CurrentPosition + (int)NameLength);

            UInt32 LastNameLength = buffer.ReadUInt32();
            outputStream.WriteLine("{0, -5}: LastName Length: {1}", buffer.GetPosition() - 4, LastNameLength);

            CurrentPosition = buffer.GetPosition();
            outputStream.WriteLine("{0, -5}: Last Name: {1}", buffer.GetPosition(), buffer.ReadString(false));

            buffer.SetPosition(CurrentPosition + (int)LastNameLength);

            outputStream.WriteLine("{0, -5}: Birthday {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Account Start Date {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Last Login Date {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Time Played Minutes {1}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Time Entitled On Account {1}", buffer.GetPosition(), buffer.ReadUInt32());
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
                buffer.GetPosition(), buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle(), buffer.ReadSingle());

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

            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
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
            outputStream.WriteLine("{0, -5}: Personal Tribute Count {1}", buffer.GetPosition() - 4, PersonalTributeCount);
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
            outputStream.WriteLine("{0, -5}: Unknown6 {1} LDON Stuff ?", buffer.GetPosition() - 4, Unknown6);

            for (int i = 0; i < Unknown6; ++i)
                outputStream.WriteLine("{0, -5}: Unknown LDON? {1}", buffer.GetPosition(), buffer.ReadUInt32());


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
            outputStream.WriteLine("Skipping 50 x (String + 24 bytes) starting at offset {0}", buffer.GetPosition());
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

            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());
            outputStream.WriteLine("{0, -5}: Unknown {1:X}", buffer.GetPosition(), buffer.ReadUInt32());

            outputStream.WriteLine("Pointer is {0} bytes from end.", buffer.Length() - buffer.GetPosition());



        }
    }
}
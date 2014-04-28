//
// Copyright (C) 2001-2010 EQEMu Development Team (http://eqemulator.net). Distributed under GPL version 2.
//
//

using System;
using System.IO;
using EQExtractor2.Domain;

namespace EQExtractor2.Decoders
{
    class PatchTestSep012010Decoder : PatchJuly132010Decoder
    {
        public PatchTestSep012010Decoder()
        {
            Version = "EQ Client Build Date Test Server September 1 2010.";

            PatchConfFileName = "patch_Sep01-2010.conf";
        }

        override public IdentificationStatus Identify(int opCode, int size, PacketDirection direction)
        {
            if((opCode == OpManager.OpCodeNameToNumber("OP_ZoneEntry")) && (direction == PacketDirection.ClientToServer))
                return IdentificationStatus.Yes;

            return IdentificationStatus.No;
        }

        override public Item DecodeItemPacket(byte[] packetBuffer)
        {
            ByteStream Buffer = new ByteStream(packetBuffer);

            Item NewItem = new Item();

            NewItem.StackSize = Buffer.ReadUInt32();             // 00
            Buffer.SkipBytes(4);
            NewItem.Slot = Buffer.ReadUInt32();                  // 08
            Buffer.SkipBytes(1);
            NewItem.MerchantSlot = Buffer.ReadByte();            // 13
            NewItem.Price = Buffer.ReadUInt32();                 // 14
            Buffer.SkipBytes(5);
            NewItem.Quantity = Buffer.ReadInt32();               // 23
            Buffer.SetPosition(71);
            NewItem.Name = Buffer.ReadString(true);
            NewItem.Lore = Buffer.ReadString(true);
            NewItem.IDFile = Buffer.ReadString(true);
            NewItem.ID = Buffer.ReadUInt32();

            return NewItem;
        }

        public override void RegisterExplorers()
        {
            base.RegisterExplorers();

            //OpManager.RegisterExplorer("OP_CharInventory", ExploreCharInventoryPacket);
            //OpManager.RegisterExplorer("OP_ItemPacket", ExploreItemPacket);
            //OpManager.RegisterExplorer("OP_MercenaryPurchaseWindow", ExploreMercenaryPurchaseWindow);
        }

        public override void ExploreCharInventoryPacket(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            UInt32 ItemCount = buffer.ReadUInt32();

            outputStream.WriteLine("There are {0} items in the inventory.\r\n", ItemCount );

            for (int i = 0; i < ItemCount; ++i)
            {
                ExploreSubItem(outputStream, ref buffer);
            }

            outputStream.WriteLine("");
        }

        public override void ExploreItemPacket(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            buffer.SkipBytes(4);    // Skip type field.

            ExploreSubItem(outputStream, ref buffer);

            outputStream.WriteLine("");
        }

        public override void ExploreSubItem(StreamWriter outputStream, ref ByteStream buffer)
        {
            buffer.SkipBytes(8);

            byte Area = buffer.ReadByte();
            UInt16 MainSlot = buffer.ReadUInt16();
            Int16 SubSlot = buffer.ReadInt16();
            buffer.SkipBytes(54);
            string Name = buffer.ReadString(true);

            if (SubSlot >= 0)
                outputStream.Write("  ");

            string AreaName = "Unknown";

            switch (Area)
            {
                case 0:
                    AreaName = "Personal Inventory";
                    break;
                case 1:
                    AreaName = "Bank";
                    break;
                case 2:
                    AreaName = "Shared Bank";
                    break;
                case 6:
                    AreaName = "Personal Tribute";
                    break;
                case 7:
                    AreaName = "Guild Tribute";
                    break;
                case 8:
                    AreaName = "Merchant";
                    break;
            }

            outputStream.WriteLine("Area: {0} {1} Main Slot {2,2} Sub Slot {3,3} Name {4}", Area, AreaName.PadRight(20), MainSlot, SubSlot, Name);

            buffer.ReadString(true);    // Lore
            buffer.ReadString(true);    // IDFile

            //Buffer.SkipBytes(236);  // Item Body Struct

            UInt32 ID = buffer.ReadUInt32();
            byte Weight = buffer.ReadByte();
            byte NoRent = buffer.ReadByte();
            byte NoDrop = buffer.ReadByte();
            byte Attune = buffer.ReadByte();
            byte Size = buffer.ReadByte();

            outputStream.WriteLine("   ID: {0} Weight: {1} NoRent: {2} NoDrop: {3} Attune {4} Size {5}", ID, Weight, NoRent, NoDrop, Attune, Size);

            UInt32 Slots = buffer.ReadUInt32();
            UInt32 Price = buffer.ReadUInt32();
            UInt32 Icon = buffer.ReadUInt32();
            buffer.SkipBytes(2);
            UInt32 BenefitFlags = buffer.ReadUInt32();
            byte Tradeskills = buffer.ReadByte();

            outputStream.WriteLine("   Slots: {0} Price: {1} Icon: {2} BenefitFlags {3} Tradeskills: {4}", Slots, Price, Icon, BenefitFlags, Tradeskills);

            byte CR = buffer.ReadByte();
            byte DR = buffer.ReadByte();
            byte PR = buffer.ReadByte();
            byte MR = buffer.ReadByte();
            byte FR = buffer.ReadByte();
            byte SVC = buffer.ReadByte();

            outputStream.WriteLine("   CR: {0} DR: {1} PR: {2} MR: {3} FR: {4} SVC: {5}", CR, DR, PR, MR, FR, SVC);

            byte AStr = buffer.ReadByte();
            byte ASta = buffer.ReadByte();
            byte AAgi = buffer.ReadByte();
            byte ADex = buffer.ReadByte();
            byte ACha = buffer.ReadByte();
            byte AInt = buffer.ReadByte();
            byte AWis = buffer.ReadByte();

            outputStream.WriteLine("   AStr: {0} ASta: {1} AAgi: {2} ADex: {3} ACha: {4} AInt: {5} AWis: {6}", AStr, ASta, AAgi, ADex, ACha, AInt, AWis);

            Int32 HP = buffer.ReadInt32();
            Int32 Mana = buffer.ReadInt32();
            UInt32 Endurance = buffer.ReadUInt32();
            Int32 AC = buffer.ReadInt32();
            Int32 Regen = buffer.ReadInt32();
            Int32 ManaRegen = buffer.ReadInt32();
            Int32 EndRegen = buffer.ReadInt32();
            UInt32 Classes = buffer.ReadUInt32();
            UInt32 Races = buffer.ReadUInt32();
            UInt32 Deity = buffer.ReadUInt32();
            Int32 SkillModValue = buffer.ReadInt32();
            buffer.SkipBytes(4);
            UInt32 SkillModType = buffer.ReadUInt32();
            UInt32 BaneDamageRace = buffer.ReadUInt32();
            UInt32 BaneDamageBody = buffer.ReadUInt32();
            UInt32 BaneDamageRaceAmount = buffer.ReadUInt32();
            Int32 BaneDamageAmount = buffer.ReadInt32();
            byte Magic = buffer.ReadByte();
            Int32 CastTime = buffer.ReadInt32();
            UInt32 ReqLevel = buffer.ReadUInt32();
            UInt32 RecLevel = buffer.ReadUInt32();
            UInt32 ReqSkill = buffer.ReadUInt32();
            UInt32 BardType = buffer.ReadUInt32();
            Int32 BardValue = buffer.ReadInt32();
            byte Light = buffer.ReadByte();
            byte Delay = buffer.ReadByte();
            byte ElemDamageAmount = buffer.ReadByte();
            byte ElemDamageType = buffer.ReadByte();
            byte Range = buffer.ReadByte();
            UInt32 Damage = buffer.ReadUInt32();
            UInt32 Color = buffer.ReadUInt32();
            byte ItemType = buffer.ReadByte();
            UInt32 Material = buffer.ReadUInt32();
            buffer.SkipBytes(4);
            UInt32 EliteMaterial = buffer.ReadUInt32();
            float SellRate = buffer.ReadSingle();
            Int32 CombatEffects = buffer.ReadInt32();
            Int32 Shielding = buffer.ReadInt32();
            Int32 StunResist = buffer.ReadInt32();
            Int32 StrikeThrough = buffer.ReadInt32();
            Int32 ExtraDamageSkill = buffer.ReadInt32();
            Int32 ExtraDamageAmount = buffer.ReadInt32();
            Int32 SpellShield = buffer.ReadInt32();
            Int32 Avoidance = buffer.ReadInt32();
            Int32 Accuracy = buffer.ReadInt32();
            UInt32 CharmFileID = buffer.ReadUInt32();
            UInt32 FactionMod1 = buffer.ReadUInt32();
            Int32 FactionAmount1 = buffer.ReadInt32();
            UInt32 FactionMod2 = buffer.ReadUInt32();
            Int32 FactionAmount2 = buffer.ReadInt32();
            UInt32 FactionMod3 = buffer.ReadUInt32();
            Int32 FactionAmount3 = buffer.ReadInt32();
            UInt32 FactionMod4 = buffer.ReadUInt32();
            Int32 FactionAmount4 = buffer.ReadInt32();

            buffer.ReadString(true);    // Charm File
            buffer.SkipBytes(64);   // Item Secondary Body Struct
            buffer.ReadString(true);    // Filename
            buffer.SkipBytes(76);   // Item Tertiary Body Struct
            buffer.SkipBytes(30);   // Click Effect Struct
            buffer.ReadString(true);    // Clickname
            buffer.SkipBytes(4);    // clickunk7
            buffer.SkipBytes(30);   // Proc Effect Struct
            buffer.ReadString(true);    // Proc Name
            buffer.SkipBytes(4);    // unknown5
            buffer.SkipBytes(30);   // Worn Effect Struct
            buffer.ReadString(true);    // Worn Name
            buffer.SkipBytes(4);    // unknown6
            buffer.SkipBytes(30);   // Worn Effect Struct
            buffer.ReadString(true);    // Worn Name
            buffer.SkipBytes(4);    // unknown6
            buffer.SkipBytes(30);   // Worn Effect Struct
            buffer.ReadString(true);    // Worn Name
            buffer.SkipBytes(4);    // unknown6
            buffer.SkipBytes(30);   // Worn Effect Struct
            buffer.ReadString(true);    // Worn Name
            buffer.SkipBytes(4);    // unknown6
            buffer.SkipBytes(103);   // Item Quaternary Body Struct - 4 (we want to read the SubLength field at the end)

            UInt32 SubLengths = buffer.ReadUInt32();

            for (int i = 0; i < SubLengths; ++i)
            {
                buffer.SkipBytes(4);
                ExploreSubItem(outputStream, ref buffer);
            }
        }

        public override void ExploreMercenaryPurchaseWindow(StreamWriter OutputStream, ByteStream Buffer, PacketDirection Direction)
        {
            UInt32 TypeCount = Buffer.ReadUInt32();

            //OutputStream.WriteLine("Type Count: {0}\r\n", TypeCount);
            OutputStream.WriteLine("VARSTRUCT_ENCODE_TYPE(uint32, Buffer, {0}); // Number of Types (Journeyman and Apprentice in this case\r\n", TypeCount);
            for (int i = 0; i < TypeCount; ++i)
            {
                UInt32 TypeDBStringID = Buffer.ReadUInt32();
                //OutputStream.WriteLine("  Type {0} DBStringID {1}", i, TypeDBStringID);
                OutputStream.WriteLine("VARSTRUCT_ENCODE_TYPE(uint32, Buffer, {0}); // DBStringID for Type {1}", TypeDBStringID, i);
            }

            UInt32 Count2 = Buffer.ReadUInt32();

            //OutputStream.WriteLine("  Count 2 is {0}", Count2);
            OutputStream.WriteLine("VARSTRUCT_ENCODE_TYPE(uint32, Buffer, {0}); // Count of Sub-types that follow", Count2);

            for (int i = 0; i < Count2; ++i)
            {
                int Offset = Buffer.GetPosition();

                UInt32 Unknown1 = Buffer.ReadUInt32();
                OutputStream.WriteLine("VARSTRUCT_ENCODE_TYPE(uint32, Buffer, {0}); // Unknown", Unknown1);
                UInt32 DBStringID1 = Buffer.ReadUInt32();
                OutputStream.WriteLine("VARSTRUCT_ENCODE_TYPE(uint32, Buffer, {0}); // DBStringID of Type", DBStringID1);
                UInt32 DBStringID2 = Buffer.ReadUInt32();
                OutputStream.WriteLine("VARSTRUCT_ENCODE_TYPE(uint32, Buffer, {0}); // DBStringID of Sub-Type", DBStringID2);
                UInt32 PurchaseCost = Buffer.ReadUInt32();
                OutputStream.WriteLine("VARSTRUCT_ENCODE_TYPE(uint32, Buffer, {0}); // Purchase Cost", PurchaseCost);
                UInt32 UpkeepCost = Buffer.ReadUInt32();
                OutputStream.WriteLine("VARSTRUCT_ENCODE_TYPE(uint32, Buffer, {0}); // Upkeep Cost", UpkeepCost);
                UInt32 Unknown2 = Buffer.ReadUInt32();
                OutputStream.WriteLine("VARSTRUCT_ENCODE_TYPE(uint32, Buffer, {0}); // Unknown", Unknown2);
                UInt32 Unknown3 = Buffer.ReadUInt32();
                OutputStream.WriteLine("VARSTRUCT_ENCODE_TYPE(uint32, Buffer, {0}); // Unknown", Unknown3);
                UInt32 Unknown4 = Buffer.ReadUInt32();
                OutputStream.WriteLine("VARSTRUCT_ENCODE_TYPE(uint32, Buffer, {0}); // Unknown", Unknown4);

                byte Unknown5 = Buffer.ReadByte();
                //OutputStream.WriteLine("VARSTRUCT_ENCODE_TYPE(uint8, Buffer, {0}); // Unknown", Unknown5);

                UInt32 Unknown6 = Buffer.ReadUInt32();
                //OutputStream.WriteLine("VARSTRUCT_ENCODE_TYPE(uint32, Buffer, {0}); // Unknown", Unknown6);
                UInt32 Unknown7 = Buffer.ReadUInt32();
                //OutputStream.WriteLine("VARSTRUCT_ENCODE_TYPE(uint32, Buffer, {0}); // Unknown", Unknown7);
                UInt32 Unknown8 = Buffer.ReadUInt32();
                //OutputStream.WriteLine("VARSTRUCT_ENCODE_TYPE(uint32, Buffer, {0}); // Unknown", Unknown8);

                UInt32 StanceCount = Buffer.ReadUInt32();

                OutputStream.WriteLine("VARSTRUCT_ENCODE_TYPE(uint32, Buffer, {0}); // Number of Stances for this Merc", StanceCount);

                UInt32 Unknown10 = Buffer.ReadUInt32();
                OutputStream.WriteLine("VARSTRUCT_ENCODE_TYPE(uint32, Buffer, {0}); // Unknown", Unknown10);

                byte Unknown11 = Buffer.ReadByte();
                //OutputStream.WriteLine("VARSTRUCT_ENCODE_TYPE(uint8, Buffer, {0}); // Unknown", Unknown11);


                //OutputStream.WriteLine("   Offset: {5} Unknown1: {0} DBStrings: {1} {2} Purchase: {3} Upkeep: {4}\r\n", Unknown1, DBStringID1, DBStringID2,
                //                PurchaseCost, UpkeepCost, Offset);
                //OutputStream.WriteLine("   Unknowns: {0} {1} {2} {3} {4} {5} {6} {7} {8}\r\n",
                //                Unknown2, Unknown3, Unknown4, Unknown5, Unknown6, Unknown7, Unknown8, Unknown10, Unknown11);

                //OutputStream.WriteLine("    Stance Count: {0}", StanceCount);

                for (int j = 0; j < StanceCount; ++j)
                {
                    UInt32 StanceNum = Buffer.ReadUInt32();
                    OutputStream.WriteLine("VARSTRUCT_ENCODE_TYPE(uint32, Buffer, {0}); // Stance Number", StanceNum);
                    UInt32 StanceType = Buffer.ReadUInt32();
                    OutputStream.WriteLine("VARSTRUCT_ENCODE_TYPE(uint32, Buffer, {0}); // Stance DBStringID (1 = Passive, 2 = Balanced etc.", StanceType);

                    //OutputStream.WriteLine("     {0}: {1}", StanceNum, StanceType);
                }
                OutputStream.WriteLine("");
            }

            OutputStream.WriteLine("\r\nBuffer position at end is {0}", Buffer.GetPosition());
            OutputStream.WriteLine("");
        }


    }
}

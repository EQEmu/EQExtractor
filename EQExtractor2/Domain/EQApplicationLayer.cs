//
// Copyright (C) 2001-2010 EQEMu Development Team (http://eqemulator.net). Distributed under GPL version 2.
//
//
// The code in here should all be independent of the version of the EQ client.
//
// The EQStreamProcessor class will call the relevant Patch Specific decoder to return the required data in order to build SQL
//


using System;
using System.Collections.Generic;
using EQExtractor2.Decoders;

namespace EQExtractor2.Domain
{
    using System.IO;
    using System.Runtime.Remoting.Messaging;

    public delegate void LogHandler(string message);

    class EQStreamProcessor
    {
        private const string ZoneLookupXmlPath = "Configs/ZoneLookups.xml";
        private static ZoneLookups _lookups;
        // PatchSpecificDecoder is the base, Dummy' class from which the actual supported patch version decoders inherit.
        // Setting PatchDecoder to this is a failsafe in case the stream isn't identified.
        public PatchSpecficDecoder PatchDecoder = new DefaultPatchSpecficDecoderImpl();

        List<ZonePoint> ZonePointList { get; set; }

        // The PacketManager decodes the raw stream into EQApplictionPackets and holds the list of decoded packets for us to process
        public PacketManager Packets { get; set; }

        // The PatchList is a list of each client version specific decoder
        List<PatchSpecficDecoder> PatchList { get; set; }

        // EQStreamProcessor is the class that generates the SQL. It calls the relevant patch specific decoder to decode the packets
        // and return the relevant data in a standardized internal format.

        public EQStreamProcessor()
        {
            ZonePointList = null;
            if (!File.Exists(ZoneLookupXmlPath)) throw new FileNotFoundException("Unable to proceed without a ZoneLookups.xml file. I was looking here for it:- "+Path.Combine(Environment.CurrentDirectory+ZoneLookupXmlPath));
            if (_lookups == null) _lookups = ZoneLookups.Deserialize(ZoneLookupXmlPath);
            Packets = new PacketManager();

            // Tell the PacketManager to call our Identify method to identify the packet stream. We will then call the different
            // patch specific Identifiers to identify the patch version.

            Packets.SetVersionIdentifierMethod(Identify);

            // Put our supported patch version decoders into the list.

            PatchList = new List<PatchSpecficDecoder>();
            PatchList.Add(new PatchMay192014Decoder());
            PatchList.Add(new PatchApr242014Decoder());
            PatchList.Add(new PatchTestApr242014Decoder());

            PatchList.Add(new PatchApril032014Decoder());

            PatchList.Add(new PatchMay122010Decoder());

            PatchList.Add(new PatchJuly132010Decoder());

            PatchList.Add(new PatchTestSep012010Decoder());

            PatchList.Add(new PatchTestSep222010Decoder());

            PatchList.Add(new PatchOct202010Decoder());

            PatchList.Add(new PatchDec072010Decoder());

            PatchList.Add(new PatchFeb082011Decoder());

            PatchList.Add(new PatchMarch152011Decoder());

            PatchList.Add(new PatchMay122011Decoder());

            PatchList.Add(new PatchMay242011Decoder());

            PatchList.Add(new PatchAug042011Decoder());

            PatchList.Add(new PatchNov172011Decoder());

            PatchList.Add(new PatchMar152012Decoder());

            PatchList.Add(new PatchJune252012Decoder());

            PatchList.Add(new PatchJuly132012Decoder());

            PatchList.Add(new PatchAugust152012Decoder());

            PatchList.Add(new PatchDecember102012Decoder());

            PatchList.Add(new PatchJanuary162013Decoder());

            PatchList.Add(new PatchTestServerJanuary162013Decoder());

            PatchList.Add(new PatchTestServerFebruary52013Decoder());

            PatchList.Add(new PatchFebruary112013Decoder());

            PatchList.Add(new PatchMarch132013Decoder());

            PatchList.Add(new PatchApril152013Decoder());

            PatchList.Add(new PatchSoD());

        }

        // This is called from the main form to tell us where the application was launched from (where to look for the .conf files)
        // and also gives us the LogHandler method so we can send debug messages to the main form.

        public bool Init(string confDirectory, Action<string> logger)
        {
            // Pass the LogHandler on down to the PacketManager

            Packets.SetLogHandler(logger);

            var errorMessage = "";

            // Here we init all the patch version specific decoders. The only reason one should fail to init is if it can't
            // find it's patch_XXXX.conf.
            //
            // If at least one initialises successfully, then return true  to the caller, otherwise return false
            var allDecodersFailed = true;

            PatchDecoder.Init(confDirectory, ref errorMessage);

            foreach (var p in PatchList)
            {
                logger("Initialising patch " + p.GetVersion());
                if (!p.Init(confDirectory, ref errorMessage))
                    logger(errorMessage);
                else
                    allDecodersFailed = false;
            }

            return !allDecodersFailed;
        }

        public string GetDecoderVersion()
        {
            // We don't need to check if PatchDecoder is null, because it is always initialised to an instance of the base
            // PatchSpecficDecoder class
            return PatchDecoder.GetVersion();
        }

        public bool StreamRecognised()
        {
            // Only the base PatchSpecficDecoder class returns false
            return !PatchDecoder.UnsupportedVersion();
        }

        public bool DumpPackets(string FileName, bool ShowTimeStamps)
        {
            return PatchDecoder.DumpPackets(FileName, ShowTimeStamps);
        }

        // This method is called by the PacketManager as it processes each packet in order to determine if the client patch
        // version is recognised.
        //
        // We call the Identify methods of each supported version decoder
        //
        // The decoders return No, if they cannot recognise the version from the given OpCode, size and direction,
        // 'Tentative' if they recognise it, but are not 100% sure it is their version, and Yes if they are 100%
        // sure the version has been identified.
        //
        public IdentificationStatus Identify(int opCode, int size, PacketDirection direction)
        {
            var status = IdentificationStatus.No;

            foreach (var p in PatchList)
            {
                var tempStatus = p.Identify(opCode, size, direction);

                if (tempStatus == IdentificationStatus.Yes)
                {
                    // The version has been identified. Set PatchDecoder to point to this decoder
                    PatchDecoder = p;

                    return IdentificationStatus.Yes;
                }
                else if (tempStatus > status)
                    status = tempStatus;
            }
            return status;
        }

        // This is called by the main form when all the packets have been processed. It prompts us to pass the packets down
        // to the decoder for this patch version.
        public void PCAPFileReadFinished()
        {
            PatchDecoder.GivePackets(Packets);
        }

        // The following methods are called by the main form and we just pass them on down to the decoder for the particular
        // patch version that the .pcap was produced with.

        public List<byte[]> GetPacketsOfType(string opCodeName, PacketDirection direction)
        {
            return PatchDecoder.GetPacketsOfType(opCodeName, direction);
        }

        public void ProcessPacket(System.Net.IPAddress srcIp, System.Net.IPAddress dstIp, ushort srcPort, ushort dstPort, byte[] Payload, DateTime PacketTime)
        {
            Packets.ProcessPacket(srcIp, dstIp, srcPort, dstPort, Payload, PacketTime, false, false);
        }

        public DateTime GetCaptureStartTime()
        {
            return PatchDecoder.GetCaptureStartTime();
        }

        public string GetZoneName()
        {
            return PatchDecoder.GetZoneName();
        }

        public string GetZoneLongName()
        {
            var newZone = PatchDecoder.GetZoneData();

            return newZone.LongName;
        }

        public int VerifyPlayerProfile()
        {
            return PatchDecoder.VerifyPlayerProfile();
        }

        public UInt16 GetZoneNumber()
        {
            return PatchDecoder.GetZoneNumber();
        }

        //
        // The following are all the methods that actually generate the SQL. Again, we call the patch specific decoder's methods to
        // return the data we need in a version agnostic format.
        //

        public void GenerateDoorsSQL(string zoneName, int doorDbid, UInt32 spawnVersion, Action<string> SQLOut)
        {
            var doorList = PatchDecoder.GetDoors();

            SQLOut("--");
            SQLOut("-- Doors");
            SQLOut("--");
            int upperBound = (doorDbid == 1 ? 998 : 999);
            SQLOut("DELETE from doors where zone = '" + zoneName + "' and doorid >= @BaseDoorID and doorid <= @BaseDoorID + " + upperBound + " and version = " + spawnVersion + ";");

            foreach(var d in doorList)
            {
                if ((d.OpenType == 57) || (d.OpenType == 58))
                {
                    var zp = GetZonePointNumber(d.DoorParam);

                    if (zp != null)
                    {
                        d.DestZone = _lookups.ZoneNumberToName(zp.Value.TargetZoneID);
                        d.DestX = zp.Value.TargetX;
                        d.DestY = zp.Value.TargetY;
                        d.DestZ = zp.Value.TargetZ;
                        d.DestHeading = zp.Value.Heading;

                    }
                }

                string DoorQuery = "INSERT INTO doors(`doorid`, `zone`, `version`, `name`, `pos_y`, `pos_x`, `pos_z`, `heading`, `opentype`, `doorisopen`, `door_param`, `dest_zone`, `dest_x`, `dest_y`, `dest_z`, `dest_heading`, `invert_state`, `incline`, `size`) VALUES(";
                DoorQuery += "@BaseDoorID + " + d.ID + ", '" + zoneName + "', " + spawnVersion + ", '" + d.Name + "', " + d.YPos + ", " + d.XPos + ", " + d.ZPos + ", " + d.Heading + ", " + d.OpenType + ", " + d.StateAtSpawn + ", " + d.DoorParam + ", '" + d.DestZone + "', " + d.DestX + ", " + d.DestY + ", " + d.DestZ + ", " + d.DestHeading + ", " + d.InvertState + ", " + d.Incline + ", " + d.Size + ");";

                SQLOut(DoorQuery);
            }
        }

        public void GenerateSpawnSQL(bool GenerateSpawns, bool GenerateGrids, bool GenerateMerchants,
                                            string ZoneName, UInt32 ZoneID, UInt32 SpawnVersion,
                                            bool UpdateExistingNPCTypes, bool UseNPCTypesTint, string SpawnNameFilter,
                                            bool CoalesceWaypoints, bool IncludeInvisibleMen, Action<string> SQLOut)
        {
            UInt32 NPCTypeDBID = 0;
            UInt32 SpawnGroupID = 0;
            UInt32 SpawnEntryID = 0;
            UInt32 Spawn2ID = 0;
            UInt32 GridDBID = 0;
            UInt32 MerchantDBID = 0;

            List<ZoneEntryStruct> ZoneSpawns = PatchDecoder.GetSpawns();

            List<UInt32> FindableEntities = PatchDecoder.GetFindableSpawns();

            if (GenerateSpawns)
            {
                SQLOut("--");
                SQLOut("-- Spawns");
                SQLOut("--");
                if (SpawnVersion == 0)
                {
                    SQLOut("DELETE from npc_types where id >= @StartingNPCTypeID and id <= @StartingNPCTypeID + 999 and version = " + SpawnVersion + ";");

                    if(UseNPCTypesTint)
                        SQLOut("DELETE from npc_types_tint where id >= @StartingNPCTypeID and id <= @StartingNPCTypeID + 999;");

                    SQLOut("DELETE from spawngroup where id IN (select spawnGroupID from spawnentry where npcid between @StartingSpawnEntryID AND @StartingSpawnEntryID+999);");
                    SQLOut("DELETE from spawn2 where spawnGroupID IN (select spawnGroupID from spawnentry where npcid between @StartingSpawnEntryID AND @StartingSpawnEntryID+999) and version = " + SpawnVersion + ";");
                    SQLOut("DELETE from spawnentry where npcid between @StartingSpawnEntryID AND @StartingSpawnEntryID+999;");
                }
                else
                {
                    SQLOut("DELETE from npc_types where id >= @StartingNPCTypeID and id <= @StartingNPCTypeID + 99 and version = " + SpawnVersion + ";");
                    SQLOut("DELETE from spawngroup where id IN (select spawnGroupID from spawnentry where npcid between @StartingSpawnEntryID AND @StartingSpawnEntryID+99);");
                    SQLOut("DELETE from spawn2 where spawnGroupID IN (select spawnGroupID from spawnentry where npcid between @StartingSpawnEntryID AND @StartingSpawnEntryID+99) and version = " + SpawnVersion + ";");
                    SQLOut("DELETE from spawnentry where spawngroupID IN (select spawnGroupID from spawnentry where npcid between @StartingSpawnEntryID AND @StartingSpawnEntryID+99);");
                }

            }

            NPCTypeList NPCTL = new NPCTypeList();

            NPCSpawnList NPCSL = new NPCSpawnList();

            foreach(ZoneEntryStruct Spawn in ZoneSpawns)
            {
                if (NPCType.IsMount(Spawn.SpawnName))
                    continue;

                if (!IncludeInvisibleMen && (Spawn.Race == 127))
                    continue;

                Spawn.Findable = (FindableEntities.IndexOf(Spawn.SpawnID) >= 0);

                if (Spawn.IsNPC != 1)
                    continue;

                if (Spawn.PetOwnerID > 0)
                    continue;

                if ((SpawnNameFilter.Length > 0) && (Spawn.SpawnName.IndexOf(SpawnNameFilter) == -1))
                    continue;

                bool ColoursInUse = false;

                for (int ColourSlot = 0; ColourSlot < 9; ++ColourSlot)
                {
                    if (((Spawn.SlotColour[ColourSlot] & 0x00ffffff) != 0) && UseNPCTypesTint)
                        ColoursInUse = true;
                }

                if (Spawn.IsMercenary > 0)
                    continue;

                UInt32 ExistingDBID = NPCTL.FindNPCType(Spawn.SpawnName, Spawn.Level, Spawn.Gender, Spawn.Size, Spawn.Face, Spawn.WalkSpeed, Spawn.RunSpeed, Spawn.Race,
                       Spawn.BodyType, Spawn.HairColor, Spawn.BeardColor, Spawn.EyeColor1, Spawn.EyeColor2, Spawn.HairStyle, Spawn.Beard,
                       Spawn.DrakkinHeritage, Spawn.DrakkinTattoo, Spawn.DrakkinDetails, Spawn.Deity, Spawn.Class, Spawn.EquipChest2,
                       Spawn.Helm, Spawn.LastName);


                if (ExistingDBID == 0)
                {
                    NPCType NewNPCType = new NPCType(NPCTypeDBID, Spawn.SpawnName, Spawn.Level, Spawn.Gender, Spawn.Size, Spawn.Face, Spawn.WalkSpeed, Spawn.RunSpeed, Spawn.Race,
                       Spawn.BodyType, Spawn.HairColor, Spawn.BeardColor, Spawn.EyeColor1, Spawn.EyeColor2, Spawn.HairStyle, Spawn.Beard,
                       Spawn.DrakkinHeritage, Spawn.DrakkinTattoo, Spawn.DrakkinDetails, Spawn.Deity, Spawn.Class, Spawn.EquipChest2,
                       Spawn.Helm, Spawn.LastName, Spawn.Findable, Spawn.MeleeTexture1, Spawn.MeleeTexture2, Spawn.ArmorTintRed, Spawn.ArmorTintGreen, Spawn.ArmorTintBlue, Spawn.SlotColour,Spawn.StandState);

                    NPCTL.AddNPCType(NewNPCType);

                    ExistingDBID = NPCTypeDBID++;

                    UInt32 ArmorTintID = 0;

                    if (ColoursInUse)
                    {
                        ArmorTintID = ExistingDBID;
                        Spawn.ArmorTintRed = 0;
                        Spawn.ArmorTintGreen = 0;
                        Spawn.ArmorTintBlue = 0;
                    }

                    string NPCTypesQuery = "INSERT INTO npc_types(`id`, `name`, `lastname`, `level`, `gender`, `size`, `runspeed`,`race`, `class`, `bodytype`, `hp`, `texture`, `helmtexture`, `face`, `luclin_hairstyle`, `luclin_haircolor`, `luclin_eyecolor`, `luclin_eyecolor2`,`luclin_beard`, `luclin_beardcolor`, `findable`, `version`, `d_meele_texture1`, `d_meele_texture2`, `armortint_red`, `armortint_green`, `armortint_blue`, `drakkin_heritage`, `drakkin_tattoo`, `drakkin_details`,`special_abilities`,`slow_mitigation`) VALUES(";

                    NPCTypesQuery += "@StartingNPCTypeID + " + ExistingDBID + ", '" + Spawn.SpawnName + "', " + "'" + Spawn.LastName + "', " + Spawn.Level + ", " + Spawn.Gender + ", " + Spawn.Size + ", ";
                    NPCTypesQuery += Spawn.RunSpeed + ", " + Spawn.Race + ", " + Spawn.Class + ", " + Spawn.BodyType + ", " + Spawn.Level * (10 + Spawn.Level) + ", ";
                    NPCTypesQuery += Spawn.EquipChest2 + ", " + Spawn.Helm + ", " + Spawn.Face + ", " + Spawn.HairStyle + ", " + Spawn.HairColor + ", " + Spawn.EyeColor1 + ", ";
                    NPCTypesQuery += Spawn.EyeColor2 + ", " + Spawn.Beard + ", " + Spawn.BeardColor + ", " + (Spawn.Findable ? 1 : 0) + ", " + SpawnVersion + ", ";
                    NPCTypesQuery += Spawn.MeleeTexture1 + ", " + Spawn.MeleeTexture2 + ", " + Spawn.ArmorTintRed + ", " + Spawn.ArmorTintGreen + ", " + Spawn.ArmorTintBlue + ", ";
                    NPCTypesQuery += Spawn.DrakkinHeritage + ", " + Spawn.DrakkinTattoo + ", " + Spawn.DrakkinDetails + ",'',0);";

                    if (GenerateSpawns)
                        SQLOut(NPCTypesQuery);

                    if (GenerateSpawns && ColoursInUse && NPCType.IsPlayableRace(Spawn.Race))
                    {
                        string TintQuery = "REPLACE INTO npc_types_tint(id, tint_set_name, ";
                        TintQuery += "red1h, grn1h, blu1h, ";
                        TintQuery += "red2c, grn2c, blu2c, ";
                        TintQuery += "red3a, grn3a, blu3a, ";
                        TintQuery += "red4b, grn4b, blu4b, ";
                        TintQuery += "red5g, grn5g, blu5g, ";
                        TintQuery += "red6l, grn6l, blu6l, ";
                        TintQuery += "red7f, grn7f, blu7f, ";
                        TintQuery += "red8x, grn8x, blu8x, ";
                        TintQuery += "red9x, grn9x, blu9x) values(@StartingNPCTypeID + " + ExistingDBID + ", '" + Spawn.SpawnName + "'";

                        for (int sc = 0; sc < 9; ++sc)
                        {
                            TintQuery += String.Format(", {0}, {1}, {2}", (Spawn.SlotColour[sc] >> 16) & 0xff,
                                                                           (Spawn.SlotColour[sc] >> 8) & 0xff,
                                                                           (Spawn.SlotColour[sc] & 0xff));
                        }

                        TintQuery += ");";
                        SQLOut(TintQuery);
                        SQLOut("UPDATE npc_types SET armortint_id = @StartingNPCTypeID + " + ArmorTintID + " WHERE id = @StartingNPCTypeID + " + ExistingDBID + " LIMIT 1;");
                    }

                }

                NPCSL.AddNPCSpawn(Spawn.SpawnID, Spawn2ID, ExistingDBID, Spawn.SpawnName);

                Position p = new Position(Spawn.XPos, Spawn.YPos, Spawn.ZPos, Spawn.Heading, DateTime.MinValue);

                NPCSL.AddWaypoint(Spawn.SpawnID, p, false);

                string SpawnGroupQuery = "INSERT INTO spawngroup(`id`, `name`, `spawn_limit`, `dist`, `max_x`, `min_x`, `max_y`, `min_y`, `delay`) VALUES(";
                SpawnGroupQuery += "@StartingSpawnGroupID + " + SpawnGroupID + ", CONCAT('" + ZoneName + "', @StartingSpawnGroupID + " + SpawnGroupID + "), 0, 0, 0, 0, 0, 0, 0);";

                string SpawnEntryQuery = "INSERT INTO spawnentry(`spawngroupID`, `npcID`, `chance`) VALUES(";
                SpawnEntryQuery += "@StartingSpawnEntryID + " + SpawnEntryID + ", @StartingNPCTypeID + " + ExistingDBID + ", " + "100);";

                string Spawn2EntryQuery = "INSERT INTO spawn2(`id`, `spawngroupID`, `zone`, `version`, `x`, `y`, `z`, `heading`, `respawntime`, `variance`, `pathgrid`, `_condition`, `cond_value`, `enabled`,`animation`) VALUES(";
                Spawn2EntryQuery += "@StartingSpawn2ID + " + Spawn2ID + ", @StartingSpawnGroupID + " + SpawnGroupID + ", '" + ZoneName + "', " + SpawnVersion + ", " + Spawn.XPos + ", " + Spawn.YPos + ", " + Spawn.ZPos + ", ";
                Spawn2EntryQuery += Spawn.Heading + ", 640, 0, 0, 0, 1, 1"+","+ StandStateMapper.MapEqStandStateToEmuAnimation(Spawn.StandState).ToString()+");";

                SpawnGroupID++;
                SpawnEntryID++;
                Spawn2ID++;

                if (GenerateSpawns)
                {
                    SQLOut(SpawnGroupQuery);
                    SQLOut(SpawnEntryQuery);
                    SQLOut(Spawn2EntryQuery);
                }
            }

            if (UpdateExistingNPCTypes)
            {
                List<NPCType> UniqueSpawns = NPCTL.GetUniqueSpawns();

                foreach (NPCType n in UniqueSpawns)
                {
                    string UpdateQuery = "UPDATE npc_types set texture = {0}, helmtexture = {1}, size = {2}, face = {3}, luclin_hairstyle = {4}, ";
                    UpdateQuery += "luclin_haircolor = {5}, luclin_eyecolor = {6}, luclin_eyecolor2 = {7}, luclin_beardcolor = {8}, ";
                    UpdateQuery += "luclin_beard = {9}, drakkin_heritage = {10}, drakkin_tattoo = {11}, drakkin_details = {12}, ";
                    UpdateQuery += "armortint_red = {13}, armortint_green = {14}, armortint_blue = {15}, d_meele_texture1 = {16}, ";
                    UpdateQuery += "d_meele_texture2 = {17}, findable = {18}, gender = {20} where name = '{19}' and id >= @StartingNPCTypeID and id <= @StartingNPCTypeID + 999 and version = {21};";

                    SQLOut(String.Format(UpdateQuery, n.EquipChest2, n.Helm, n.Size, n.Face, n.HairStyle, n.HairColor, n.EyeColor1, n.EyeColor2, n.BeardColor,
                                                      n.Beard, n.DrakkinHeritage, n.DrakkinTattoo, n.DrakkinDetails, n.ArmorTintRed, n.ArmorTintGreen,
                                                      n.ArmorTintBlue, n.MeleeTexture1, n.MeleeTexture2, n.Findable, n.Name, n.Gender, SpawnVersion));

                    if(!NPCType.IsPlayableRace(n.Race))
                        continue;

                    bool ColoursInUse = false;

                    for (int ColourSlot = 0; ColourSlot < 9; ++ColourSlot)
                    {
                        if (((n.SlotColour[ColourSlot] & 0x00ffffff) != 0) && UseNPCTypesTint)
                            ColoursInUse = true;
                    }

                    if (ColoursInUse)
                    {
                        string TintQuery = "REPLACE INTO npc_types_tint(id, tint_set_name, ";
                        TintQuery += "red1h, grn1h, blu1h, ";
                        TintQuery += "red2c, grn2c, blu2c, ";
                        TintQuery += "red3a, grn3a, blu3a, ";
                        TintQuery += "red4b, grn4b, blu4b, ";
                        TintQuery += "red5g, grn5g, blu5g, ";
                        TintQuery += "red6l, grn6l, blu6l, ";
                        TintQuery += "red7f, grn7f, blu7f, ";
                        TintQuery += "red8x, grn8x, blu8x, ";
                        TintQuery += "red9x, grn9x, blu9x) SELECT id, '" + n.Name + "'";

                        for (int sc = 0; sc < 9; ++sc)
                        {
                            TintQuery += String.Format(", {0}, {1}, {2}", (n.SlotColour[sc] >> 16) & 0xff,
                                                                           (n.SlotColour[sc] >> 8) & 0xff,
                                                                           (n.SlotColour[sc] & 0xff));
                        }

                        TintQuery += " from npc_types where name = '" + n.Name + "' and id >= @StartingNPCTypeID and id <= @StartingNPCTypeID + 999 and version = " + SpawnVersion + " LIMIT 1;";
                        SQLOut(TintQuery);
                        SQLOut(String.Format("UPDATE npc_types set armortint_id = (SELECT id from npc_types_tint WHERE tint_set_name = '{0}' and id >= @StartingNPCTypeID and id <= @StartingNPCTypeID + 999 and version = {1} LIMIT 1), armortint_red = 0, armortint_green = 0, armortint_blue = 0 WHERE name = '{0}' and id >= @StartingNPCTypeID and id <= @StartingNPCTypeID + 999 and version = {1} LIMIT 1;",
                            n.Name, SpawnVersion));
                    }
                }
                return;
            }

            if (GenerateGrids)
            {
                List<PositionUpdate> AllMovementUpdates = PatchDecoder.GetAllMovementUpdates();

                foreach (PositionUpdate Update in AllMovementUpdates)
                    NPCSL.AddWaypoint(Update.SpawnID, Update.p, Update.HighRes);

                SQLOut("--");
                SQLOut("-- Grids");
                SQLOut("--");

                SQLOut("DELETE from grid WHERE id >= @StartingGridID AND id <= @StartingGridID + 999;");
                SQLOut("DELETE from grid_entries WHERE gridid >= @StartingGridID AND gridid <= @StartingGridID + 999;");
                foreach (NPCSpawn ns in NPCSL._NPCSpawnList)
                {
                    if (ns.Waypoints.Count > 1)
                    {
                        bool AllWaypointsTheSame = true;

                        for (int WPNumber = 0; WPNumber < ns.Waypoints.Count; ++WPNumber)
                        {
                            if (WPNumber == 0)
                                continue;
                            if ((ns.Waypoints[WPNumber].x != ns.Waypoints[WPNumber - 1].x) ||
                               (ns.Waypoints[WPNumber].y != ns.Waypoints[WPNumber - 1].y) ||
                               (ns.Waypoints[WPNumber].z != ns.Waypoints[WPNumber - 1].z))
                            {
                                AllWaypointsTheSame = false;
                            }
                        }

                        if (AllWaypointsTheSame)
                            continue;

                        int WaypointsInserted = 0;

                        int WPNum = 1;

                        int Pause = 10;

                        int FirstUsableWaypoint = 0;

                        if (ns.DoesHaveHighResWaypoints())
                            FirstUsableWaypoint = 1;

                        for (int WPNumber = FirstUsableWaypoint; WPNumber < ns.Waypoints.Count; ++WPNumber)
                        {
                            Position p = ns.Waypoints[WPNumber];

                            if (CoalesceWaypoints)
                            {
                                if ((WPNumber > FirstUsableWaypoint) && (WPNumber < (ns.Waypoints.Count - 2)))
                                {
                                    Position np = ns.Waypoints[WPNumber + 1];

                                    if ((Math.Abs(p.heading - np.heading) < 1) || (Math.Abs(p.heading - np.heading) > 255))
                                    {
                                        // Skipping waypoint as heading is the same as next.
                                        continue;
                                    }
                                    if ((Math.Abs(p.heading - np.heading) < 5) || (Math.Abs(p.heading - np.heading) > 251))
                                    {
                                        // Setting pause to 0 because headings are similar
                                        Pause = 0;
                                    }
                                    else if ((p.x == np.x) && (p.y == np.y) && (p.z == np.z))
                                    {
                                        // Skipping waypoint as same as next");
                                        continue;
                                    }
                                    else
                                        Pause = 10;
                                }
                            }

                            // If this is the last waypoint, and we haven't inserted any of the previous ones, then don't bother
                            // with this one either.
                            if ((WPNumber == (ns.Waypoints.Count - 1)) && (WaypointsInserted == 0))
                                continue;

                            SQLOut("INSERT into grid_entries (`gridid`, `zoneid`, `number`, `x`, `y`, `z`, `heading`, `pause`) VALUES(@StartingGridID + " + GridDBID + ", " + ZoneID + ", " + (WPNum++) + ", " + p.x + ", " + p.y + ", " + p.z + ", " + p.heading + ", " + Pause + ");");

                            ++WaypointsInserted;
                        }
                        if (WaypointsInserted > 1)
                        {
                            SQLOut("INSERT into grid(`id`, `zoneid`, `type`, `type2`) VALUES(@StartingGridID + " + GridDBID + ", " + ZoneID + ", 3, 2); -- " + ns.Name);

                            SQLOut("UPDATE spawn2 set pathgrid = @StartingGridID + " + GridDBID + " WHERE id = @StartingSpawn2ID + " + ns.Spawn2DBID + ";");

                            if (ns.DoesHaveHighResWaypoints())
                                SQLOut("UPDATE spawn2 set x = " + ns.Waypoints[1].x + ", y = " + ns.Waypoints[1].y + ", z = " + ns.Waypoints[1].z + ", heading = " + ns.Waypoints[1].heading + " WHERE id = @StartingSpawn2ID + " + ns.Spawn2DBID + ";");

                            ++GridDBID;
                        }
                    }
                }
            }
            if(GenerateMerchants)
                GenerateMerchantSQL(NPCSL, MerchantDBID, GenerateSpawns, SQLOut);
        }

        public void GenerateMerchantSQL(NPCSpawnList NPCSL, UInt32 MerchantDBID, bool GenerateSpawns, Action<string> SQLOut)
        {
            MerchantManager mm = PatchDecoder.GetMerchantData(NPCSL);

            if(GenerateSpawns)
                SQLOut("DELETE from merchantlist where merchantid >= @StartingMerchantID and merchantid <= @StartingMerchantID + 999;");

            SQLOut("--");
            SQLOut("-- Merchant Lists");
            SQLOut("-- ");

            foreach (Merchant m in mm.MerchantList)
            {
                UInt32 MerchantSpawnID = m.SpawnID;

                NPCSpawn npc = NPCSL.GetNPC(MerchantSpawnID);

                if(npc == null)
                    continue;

                UInt32 MerchantNPCTypeID = npc.NPCTypeID;

                SQLOut("--");
                SQLOut("-- " + npc.Name);
                SQLOut("-- ");

                bool StartOfPlayerSoldItems = false;

                foreach (MerchantItem mi in m.Items)
                {
                    string Insert = "";

                    if (mi.Quantity >= 0)
                    {
                        if (!StartOfPlayerSoldItems)
                        {
                            StartOfPlayerSoldItems = true;
                            SQLOut("--");
                            SQLOut("-- The items below were more than likely sold to " + npc.Name + " by players. Uncomment them if you want.");
                            SQLOut("--");
                        }

                        Insert += "-- ";
                    }

                    Insert += "INSERT into merchantlist(`merchantid`, `slot`, `item`) VALUES(";

                    Insert += "@StartingMerchantID + " + MerchantDBID + ", " + mi.Slot + ", " + mi.ItemID + "); -- " + mi.Name;

                    SQLOut(Insert);
                }

                if(GenerateSpawns)
                    SQLOut("UPDATE npc_types SET merchant_id = @StartingMerchantID + " + MerchantDBID + " WHERE id = @StartingNPCTypeID + " + MerchantNPCTypeID + ";");

                ++MerchantDBID;
            }
        }

        public void GenerateZonePointSQL(string ZoneName, Action<string> SQLOut)
        {
            foreach (ZonePoint zp in ZonePointList)
            {
                string Insert = String.Format("REPLACE into zone_points(`zone`, `number`, `y`, `x`, `z`, `heading`, `target_y`, `target_x`, `target_z`, `target_heading`, `zoneinst`, `target_zone_id`, `buffer`) VALUES('{0}', {1}, 0, 0, 0, 0, {2}, {3}, {4}, {5}, {6}, {7}, 0);", ZoneName, zp.Number, zp.y, zp.x, zp.z, zp.Heading, zp.Instance, zp.ZoneID);

                SQLOut(Insert);
            }
        }

        public void GenerateZonePointList()
        {
            ZonePointList = PatchDecoder.GetZonePointList();
        }

        public ZonePoint? GetZonePointNumber(Int32 Number)
        {
            if (ZonePointList != null)
            {
                foreach (ZonePoint zp in ZonePointList)
                {
                    if (zp.Number == Number)
                        return zp;
                }
            }
            return null;
        }

        public UInt32 GenerateZoneSQL(Action<string> SQLOut)
        {
            UInt16 ZoneID = PatchDecoder.GetZoneNumber();

            NewZoneStruct NewZone = PatchDecoder.GetZoneData();

            SQLOut("--");
            SQLOut("-- Zone Config");
            SQLOut("--");
            string InsertFormat = "UPDATE zone set `short_name` = '{0}', `file_name` = '', `long_name` = '{1}', `safe_x` = {2}, `safe_y` = {3}, `safe_z` = {4}, ";
            InsertFormat += "`underworld` = {6}, `minclip` = {7}, `maxclip` = {8}, `fog_minclip` = {9}, `fog_maxclip` = {10}, ";
            InsertFormat += "`fog_blue` = {11}, `fog_red` = {12}, `fog_green` = {13}, `sky` = {14}, `ztype` = {15}, `time_type` = {16}, ";
            InsertFormat += "`fog_red2` = {17}, `fog_green2` = {18}, `fog_blue2` = {19}, `fog_minclip2` = {20}, `fog_maxclip2` = {21}, ";
            InsertFormat += "`fog_red3` = {22}, `fog_green3` = {23}, `fog_blue3` = {24}, `fog_minclip3` = {25}, `fog_maxclip3` = {26}, ";
            InsertFormat += "`fog_red4` = {27}, `fog_green4` = {28}, `fog_blue4` = {29}, `fog_minclip4` = {30}, `fog_maxclip4` = {31} WHERE zoneidnumber = {5};";

            SQLOut(String.Format(InsertFormat, NewZone.ShortName2, NewZone.LongName, NewZone.SafeX, NewZone.SafeY, NewZone.SafeZ,
                                               ZoneID, NewZone.UnderWorld, NewZone.MinClip, NewZone.MaxClip, NewZone.FogMinClip[0], NewZone.FogMaxClip[0],
                                               NewZone.FogBlue[0], NewZone.FogRed[0], NewZone.FogGreen[0], NewZone.Sky, NewZone.Type, NewZone.TimeType,
                                               NewZone.FogRed[1], NewZone.FogGreen[1], NewZone.FogBlue[1], NewZone.FogMinClip[1], NewZone.FogMaxClip[1],
                                               NewZone.FogRed[2], NewZone.FogGreen[2], NewZone.FogBlue[2], NewZone.FogMinClip[2], NewZone.FogMaxClip[2],
                                               NewZone.FogRed[3], NewZone.FogGreen[3], NewZone.FogBlue[3], NewZone.FogMinClip[3], NewZone.FogMaxClip[3]));

            SQLOut(String.Format("UPDATE zone set fog_density = {0} WHERE zoneidnumber = {1};", NewZone.FogDensity, ZoneID));
            SQLOut("--");

            return ZoneID;
        }

        public bool DumpAAs(string FileName)
        {
            return PatchDecoder.DumpAAs(FileName);
        }

        public void GenerateObjectSQL(bool DoGroundSpawns, bool DoObjects, UInt32 SpawnVersion, Action<string> SQLOut)
        {
            List<GroundSpawnStruct> GroundSpawns = PatchDecoder.GetGroundSpawns();

            UInt32 GroundSpawnDBID = 0;
            UInt32 ObjectDBID = 0;

            SQLOut("--");
            SQLOut("-- Objects and Groundspawns");
            SQLOut("--");

            if(DoGroundSpawns)
                SQLOut("DELETE from ground_spawns where id >= @StartingGroundSpawnID and id <= @StartingGroundSpawnID + 999 and version = " + SpawnVersion + ";");

            if(DoObjects)
                SQLOut("DELETE from object where id >= @StartingObjectID and id <= @StartingObjectID + 999 and version = " + SpawnVersion + ";");

            foreach(GroundSpawnStruct GroundSpawn in GroundSpawns)
            {
                String Insert;

                if (IsGroundSpawn(GroundSpawn.ObjectType) && DoGroundSpawns)
                {
                    Insert = "INSERT into ground_spawns(`id`, `zoneid`, `version`, `max_x`, `max_y`, `max_z`, `min_x`, `min_y`, `heading`, `name`, `item`, `max_allowed`, `comment`, `respawn_timer`) VALUES(";

                    Insert += "@StartingGroundSpawnID + " + (GroundSpawnDBID++) + ", " + GroundSpawn.ZoneID + ", " + SpawnVersion + ", " + GroundSpawn.x + ", " + GroundSpawn.y + ", " + GroundSpawn.z + ", " + GroundSpawn.x + ", " + GroundSpawn.y + ", " + GroundSpawn.Heading + ", '" + GroundSpawn.Name + "', 1001, 1, 'Auto generated by Collector. FIX THE ITEMID!', 300000);";

                    SQLOut(Insert);
                }
                else if(!IsGroundSpawn(GroundSpawn.ObjectType) && DoObjects)
                {
                    GroundSpawn.ObjectType = ObjectNameToType(GroundSpawn.Name);

                    Insert = "INSERT into object(`id`, `zoneid`, `version`, `xpos`, `ypos`, `zpos`, `heading`, `itemid`, `charges`, `objectname`, `type`, `icon`) VALUES(";

                    Insert += "@StartingObjectID + " + (ObjectDBID++) + ", " + GroundSpawn.ZoneID + ", " + SpawnVersion + ", " + GroundSpawn.x + ", " + GroundSpawn.y + ", " + GroundSpawn.z + ", " + GroundSpawn.Heading + ", 0, 0, '" + GroundSpawn.Name + "', " + GroundSpawn.ObjectType + ", 0);";

                    SQLOut(Insert);
                }
            }
        }

        static bool IsGroundSpawn(UInt32 type)
        {
            //vsab Korascian warrens has these types
            switch (type)
            {
                case 1:
                case 8:
                case 12:
                case 29:
                    return true;
                default:
                    return false;
            }
        }


        public static UInt32 ObjectNameToType(string Name)
        {
            switch (Name)
            {
                case "IT11208_ACTORDEF":
                    return 8;
                case "IT537_ACTORDEF":
                    return 29;
                case "IT11207_ACTORDEF":
                    return 12;
                case "IT10511_ACTORDEF":
                    return 3;
                case "IT10512_ACTODEF":
                    return 1;
                case "IT10714_ACTORDEF":
                    return 53;
                case "IT10800_ACTORDEF":
                    return 21;
                case "IT10801_ACTORDEF":
                    return 22;
                case "IT10802_ACTORDEF":
                    return 16;
                case "IT10803_ACTORDEF":
                    return 15;
                case "IT10865_ACTORDEF":
                    return 15;
                case "IT128_ACTORDEF":
                    return 16;
                case "IT27_ACTORDEF":
                    return 0;
                case "IT403_ACTORDEF":
                    return 1;
                case "IT5_ACTORDEF":
                    return 25;
                case "IT63_ACTORDEF":
                    return 8;
                case "IT64_ACTORDEF":
                    return 30;
                case "IT69_ACTORDEF":
                    return 15;
                case "IT73_ACTORDEF":
                    return 22;
                case "IT74_ACTORDEF":
                    return 21;
                case "IT10805_ACTORDEF":
                case "IT70_ACTORDEF":
                    return 19;
                case "IT66_ACTORDEF":
                case "IT10804_ACTORDEF":
                case "IT10863_ACTORDEF":
                case "IT10864_ACTORDEF":
                    return 17;

                default:
                    return 255;
            }
        }


        public bool SupportsSQLGeneration()
        {
            if (PatchDecoder.SupportsSQLGeneration)
                return true;

            return false;
        }
    }
}

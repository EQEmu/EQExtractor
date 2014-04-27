using System;
using System.Collections.Generic;
using System.IO;
using EQExtractor2.Domain;

namespace EQExtractor2.Decoders
{
    internal interface IPatchDecoder
    {
        bool Init(string confDirectory, ref string errorMessage);
        IdentificationStatus Identify(int opCode, int size, PacketDirection direction);
        bool UnsupportedVersion();
        int VerifyPlayerProfile();
        void GivePackets(PacketManager pm);
        int PacketTypeCountByName(string opCodeName);

        #region Get Methods

        UInt16 GetZoneNumber();
        List<Door> GetDoors();
        MerchantManager GetMerchantData(NPCSpawnList npcsl);      
        NewZoneStruct GetZoneData();
        List<ZoneEntryStruct> GetSpawns();
        List<PositionUpdate> GetHighResolutionMovementUpdates();
        List<PositionUpdate> GetLowResolutionMovementUpdates();
        List<PositionUpdate> GetAllMovementUpdates();
        List<PositionUpdate> GetClientMovementUpdates();
        List<GroundSpawnStruct> GetGroundSpawns();
        List<UInt32> GetFindableSpawns();
        string GetZoneName();
        List<ZonePoint> GetZonePointList();
        string GetVersion();
        List<byte[]> GetPacketsOfType(string opCodeName, PacketDirection direction);
        DateTime GetCaptureStartTime();

        #endregion

        #region Dump Methods

        bool DumpPackets(string fileName, bool showTimeStamps);
        bool DumpAAs(string fileName);

        #endregion

        #region Decode Methods

        PositionUpdate Decode_OP_NPCMoveUpdate(byte[] updatePacket);
        PositionUpdate Decode_OP_MobUpdate(byte[] mobUpdatePacket);
        Item DecodeItemPacket(byte[] packetBuffer);
        void DecodeItemPacket(StreamWriter outputStream, ByteStream buffer, PacketDirection direction);

        #endregion

        #region Explorer Methods used to figure out packet structures

        void RegisterExplorers();
        void ExploreZoneEntry(StreamWriter outputStream, ByteStream buffer, PacketDirection direction);
        void ExploreSpawnAppearance(StreamWriter outputStream, ByteStream buffer, PacketDirection direction);
        void ExploreHPUpdate(StreamWriter outputStream, ByteStream buffer, PacketDirection direction);
        void ExploreAnimation(StreamWriter outputStream, ByteStream buffer, PacketDirection direction);
        void ExploreNewZonePacket(StreamWriter outputStream, ByteStream buffer, PacketDirection direction);
        void ExplorePlayerProfile(StreamWriter outputStream, ByteStream buffer, PacketDirection direction);
        void ExploreRespawnWindow(StreamWriter outputStream, ByteStream buffer, PacketDirection direction);
        void ExploreZonePlayerToBind(StreamWriter outputStream, ByteStream buffer, PacketDirection direction);
        void ExploreRequestClientZoneChange(StreamWriter outputStream, ByteStream buffer, PacketDirection direction);
        void ExploreDeleteSpawn(StreamWriter outputStream, ByteStream buffer, PacketDirection direction);
        void ExploreCharInventoryPacket(StreamWriter outputStream, ByteStream buffer, PacketDirection direction);
        void ExploreItemPacket(StreamWriter outputStream, ByteStream buffer, PacketDirection direction);
        void ExploreInventory(StreamWriter outputStream, ByteStream buffer, PacketDirection direction);
        void ExploreTaskDescription(StreamWriter outputStream, ByteStream buffer, PacketDirection direction);
        void ExploreOpenNewTasksWindow(StreamWriter outputStream, ByteStream buffer, PacketDirection direction);
        void ExploreNPCMoveUpdate(StreamWriter outputStream, ByteStream buffer, PacketDirection direction);
        void ExploreMobUpdate(StreamWriter outputStream, ByteStream buffer, PacketDirection direction);
        void ExploreSpawnDoor(StreamWriter outputStream, ByteStream buffer, PacketDirection direction);
        void ExploreClientUpdate(StreamWriter outputStream, ByteStream buffer, PacketDirection direction);
        void ExploreMercenaryPurchaseWindow(StreamWriter outputStream, ByteStream buffer, PacketDirection direction);
        void ExploreCastSpell(StreamWriter outputStream, ByteStream buffer, PacketDirection direction);
        void ExploreMercenaryDataResponse(StreamWriter outputStream, ByteStream buffer, PacketDirection direction);
        void ExploreSubItem(StreamWriter outputStream, ref ByteStream buffer);

        #endregion
    }
}
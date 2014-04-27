using System;
using System.Collections.Generic;
using System.IO;
using EQExtractor2.Domain;

namespace EQExtractor2.Decoders
{
    public enum IdentificationStatus { No, Tentative, Yes };

    abstract class PatchSpecficDecoder: IPatchDecoder
    {
        protected class PacketToMatch
        {
	        public String	OPCodeName;
            public PacketDirection Direction;
	        public Int32	RequiredSize;
	        public bool	VersionMatched;
        };



        protected PatchSpecficDecoder()
        {
            Version = "Unsupported Client Version";
            ExpectedPPLength = 0;
            PPZoneIDOffset = 0;
            PatchConfFileName = "";
            IDStatus = IdentificationStatus.No;
            SupportsSQLGeneration = true;
        }

        public string GetVersion()
        {
            return Version;
        }

        virtual public bool UnsupportedVersion()
        {
            return ExpectedPPLength == 0;
        }

        public virtual void DecodeItemPacket(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            throw new NotImplementedException();
        }

        public virtual void ExploreInventory(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            throw new NotImplementedException();
        }

        public virtual void ExploreTaskDescription(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            throw new NotImplementedException();
        }

        public virtual void ExploreOpenNewTasksWindow(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            throw new NotImplementedException();
        }

        public virtual void ExploreNPCMoveUpdate(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            throw new NotImplementedException();
        }

        public virtual void ExploreMobUpdate(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            throw new NotImplementedException();
        }

        public virtual void ExploreSpawnDoor(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            throw new NotImplementedException();
        }

        public virtual void ExploreClientUpdate(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            throw new NotImplementedException();
        }

        public virtual void ExploreMercenaryPurchaseWindow(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            throw new NotImplementedException();
        }

        public virtual void ExploreCastSpell(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            throw new NotImplementedException();
        }

        public virtual void ExploreMercenaryDataResponse(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            throw new NotImplementedException();
        }

        virtual public bool Init(string confDirectory, ref string errorMessage)
        {
            OpManager = new OpCodeManager();

            return false;
        }

        virtual public IdentificationStatus Identify(int opCode, int size, PacketDirection direction)
        {
            return IdentificationStatus.No;
        }

        virtual public List<Door> GetDoors()
        {
            List<Door> DoorList = new List<Door>();

            return DoorList;
        }

        virtual public UInt16 GetZoneNumber()
        {
            return 0;
        }

        virtual public int VerifyPlayerProfile()
        {
            return 0;
        }

        virtual public MerchantManager GetMerchantData(NPCSpawnList npcsl)
        {
            return null;
        }

        virtual public Item DecodeItemPacket(byte[] packetBuffer)
        {
            Item NewItem = new Item();

            return NewItem;
        }


        virtual public List<ZonePoint> GetZonePointList()
        {
            List<ZonePoint> ZonePointList = new List<ZonePoint>();

            return ZonePointList;
        }

        virtual public NewZoneStruct GetZoneData()
        {
            NewZoneStruct NewZone = new NewZoneStruct();

            return NewZone;
        }

        virtual public List<ZoneEntryStruct> GetSpawns()
        {
            List<ZoneEntryStruct> ZoneSpawns = new List<ZoneEntryStruct>();

            return ZoneSpawns;
        }

        virtual public List<PositionUpdate> GetHighResolutionMovementUpdates()
        {
            List<PositionUpdate> Updates = new List<PositionUpdate>();

            return Updates;
        }

        virtual public List<PositionUpdate> GetLowResolutionMovementUpdates()
        {
            List<PositionUpdate> Updates = new List<PositionUpdate>();

            return Updates;
        }

        virtual public List<PositionUpdate> GetAllMovementUpdates()
        {
            List<PositionUpdate> Updates = new List<PositionUpdate>();

            return Updates;
        }

        virtual public  PositionUpdate Decode_OP_NPCMoveUpdate(byte[] updatePacket)
        {
            PositionUpdate PosUpdate = new PositionUpdate();

            return PosUpdate;
        }

        virtual public PositionUpdate Decode_OP_MobUpdate(byte[] mobUpdatePacket)
        {
            PositionUpdate PosUpdate = new PositionUpdate();

            return PosUpdate;
        }

        virtual public List<PositionUpdate> GetClientMovementUpdates()
        {
            List<PositionUpdate> Updates = new List<PositionUpdate>();

            return Updates;
        }

        virtual public List<GroundSpawnStruct> GetGroundSpawns()
        {
            List<GroundSpawnStruct> GroundSpawns = new List<GroundSpawnStruct>();

            return GroundSpawns;
        }

        virtual public List<UInt32> GetFindableSpawns()
        {
            List<UInt32> FindableSpawnList = new List<UInt32>();

            return FindableSpawnList;
        }

        virtual public string GetZoneName()
        {
            return "";
        }

        public virtual void ExploreItemPacket(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            throw new NotImplementedException();
        }

        virtual public bool DumpAAs(string fileName)
        {
            return false;
        }

        public void GivePackets(PacketManager pm)
        {
            Packets = pm;
        }

        virtual public void RegisterExplorers()
        {
        }

        public virtual void ExploreZoneEntry(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            throw new NotImplementedException();
        }

        public virtual void ExploreSpawnAppearance(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            throw new NotImplementedException();
        }

        public virtual void ExploreHPUpdate(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            throw new NotImplementedException();
        }

        public virtual void ExploreAnimation(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            throw new NotImplementedException();
        }

        public virtual void ExploreNewZonePacket(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            throw new NotImplementedException();
        }

        public virtual void ExplorePlayerProfile(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            throw new NotImplementedException();
        }

        public virtual void ExploreRespawnWindow(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            throw new NotImplementedException();
        }

        public virtual void ExploreZonePlayerToBind(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            throw new NotImplementedException();
        }

        public virtual void ExploreRequestClientZoneChange(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            throw new NotImplementedException();
        }

        public virtual void ExploreDeleteSpawn(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            throw new NotImplementedException();
        }

        public virtual void ExploreCharInventoryPacket(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            throw new NotImplementedException();
        }

        public virtual  List<byte[]> GetPacketsOfType(string opCodeName, PacketDirection direction)
        {
            List<byte[]> ReturnList = new List<byte[]>();

            if (OpManager == null)
                return ReturnList;

            UInt32 OpCodeNumber = OpManager.OpCodeNameToNumber(opCodeName);

            foreach (EQApplicationPacket app in Packets.PacketList)
            {
                if ((app.OpCode == OpCodeNumber) && (app.Direction == direction) && (app.Locked))
                    ReturnList.Add(app.Buffer);
        }

            return ReturnList;
        }

        public virtual DateTime GetCaptureStartTime()
        {
            if (Packets.PacketList.Count > 0)
            {
                return Packets.PacketList[0].PacketTime;
            }
            return DateTime.MinValue;
        }

        public virtual bool DumpPackets(string fileName, bool showTimeStamps)
        {

            StreamWriter PacketDumpStream;

            try
            {
                PacketDumpStream = new StreamWriter(fileName);
            }
            catch
            {
                return false;
            }

            string Direction = "";

            foreach (EQApplicationPacket p in Packets.PacketList)
            {
                if(showTimeStamps)
                    PacketDumpStream.WriteLine(p.PacketTime.ToString());

                if (p.Direction == PacketDirection.ServerToClient)
                    Direction = "[Server->Client]";
                else
                    Direction = "[Client->Server]";

                OpCode oc = OpManager.GetOpCodeByNumber(p.OpCode);

                string OpCodeName = (oc != null) ? oc.Name : "OP_Unknown";

                PacketDumpStream.WriteLine("[OPCode: 0x" + p.OpCode.ToString("x4") + "] " + OpCodeName + " " + Direction + " [Size: " + p.Buffer.Length + "]");
                PacketDumpStream.WriteLine(Utils.HexDump(p.Buffer));

                if ((oc != null) && (oc.Explorer != null))
                    oc.Explorer(PacketDumpStream, new ByteStream(p.Buffer), p.Direction);
            }

            PacketDumpStream.Close();

            return true;
        }
        public virtual int PacketTypeCountByName(string opCodeName)
        {
            UInt32 OpCodeNumber = OpManager.OpCodeNameToNumber(opCodeName);

            int Count = 0;

            foreach (EQApplicationPacket app in Packets.PacketList)
            {
                if (app.OpCode == OpCodeNumber)
                    ++Count;
            }


            return Count;
        }

        public virtual void ExploreSubItem(StreamWriter outputStream, ref ByteStream buffer)
        {
            throw new NotImplementedException();
        }

        protected virtual void AddExplorerSpawn(UInt32 ID, string Name)
        {
            ExplorerSpawnRecord e = new ExplorerSpawnRecord(ID, Name);

            ExplorerSpawns.Add(e);
        }

        protected virtual string FindExplorerSpawn(UInt32 ID)
        {
            foreach(ExplorerSpawnRecord s in ExplorerSpawns)
            {
                if (s.ID == ID)
                    return s.Name;
            }

            return "";
        }

        protected PacketManager Packets { get; set; }

        public OpCodeManager OpManager { get; set; }

        protected string Version { get; set; }

        protected int ExpectedPPLength { get; set; }

        protected int PPZoneIDOffset { get; set; }

        protected string PatchConfFileName { get; set; }

        protected PacketToMatch[] PacketsToMatch { get; set; }

        protected UInt32 WaitingForPacket { get; set; }

        protected IdentificationStatus IDStatus { get; set; }

        private List<ExplorerSpawnRecord> ExplorerSpawns = new List<ExplorerSpawnRecord>();

        public bool SupportsSQLGeneration { get; set; }
    }

    class DefaultPatchSpecficDecoderImpl : PatchSpecficDecoder
    {
    }
}

using System;

namespace EQExtractor2.Domain
{
    public class SqlGeneratorConfiguration
    {
        public bool UpdateExistingNPCTypes { get; set; }
        public string Version { get; set; }
        public UInt32 SpawnDBID { get; set; }
        public UInt32 SpawnGroupID { get; set; }
        public UInt32 SpawnEntryID { get; set; }
        public UInt32 Spawn2ID { get; set; }
        public UInt32 GridDBID { get; set; }
        public UInt32 MerchantDBID { get; set; }
        public int DoorDBID { get; set; }
        public UInt32 GroundSpawnDBID { get; set; }
        public UInt32 ObjectDBID { get; set; }
        public UInt32 ZoneID { get; set; }
        public string SpawnNameFilter { get; set; }
        public bool CoalesceWaypoints { get; set; }
        public bool GenerateZone { get; set; }
        public bool GenerateZonePoint { get; set; }
        public string ZoneName { get; set; }
        public UInt32 SpawnVersion { get; set; }
        public bool GenerateDoors { get; set; }
        public bool GenerateSpawns { get; set; }
        public bool GenerateGrids { get; set; }
        public bool GenerateMerchants{ get; set; }
        public bool UseNPCTypesTint { get; set; }
        public bool GenerateInvisibleMen { get; set; }
        public bool GenerateGroundSpawns { get; set; }
        public bool GenerateObjects { get; set; }
    }
}
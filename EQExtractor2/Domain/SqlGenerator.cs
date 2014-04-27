using System;
using System.IO;

namespace EQExtractor2.Domain
{
    class SqlGenerator
    {
        private readonly string _fileName;
        private readonly EQStreamProcessor _streamProcessor;
        private readonly SqlGeneratorConfiguration _config;
        StreamWriter SqlStream { get; set; }
        public Action<string> Log { get; private set; }
        public Action<string> SetStatus { get; private set; }

        public SqlGenerator(string fileName, EQStreamProcessor streamProcessor, SqlGeneratorConfiguration config,Action<string> logAction, Action<string> statusAction)
        {
            if (fileName == null) throw new ArgumentNullException("fileName");
            if (streamProcessor == null) throw new ArgumentNullException("streamProcessor");
            if (logAction == null) throw new ArgumentNullException("logAction");
            if (statusAction == null) throw new ArgumentNullException("statusAction");
            Log = logAction;
            SetStatus = statusAction;
            _fileName = fileName;
            _streamProcessor = streamProcessor;
            _config = config;
            SqlStream = new StreamWriter(_fileName);
        }

        public void WriteSql(string message)
        {
            SqlStream.WriteLine(message);
        }

        public void GenerateSql()
        {
            WriteSql("-- SQL created by " + _config.Version);
            WriteSql("--");
            WriteSql("-- Using Decoder: " + _streamProcessor.GetDecoderVersion());
            WriteSql("--");
            WriteSql("-- Packets captured on " + _streamProcessor.GetCaptureStartTime().ToString());
            WriteSql("--");
            WriteSql("-- Change these variables if required");
            WriteSql("--");
            WriteSql("set @StartingNPCTypeID = " + _config.SpawnDBID + ";");
            WriteSql("set @StartingSpawnGroupID = " + _config.SpawnGroupID + ";");
            WriteSql("set @StartingSpawnEntryID = " + _config.SpawnEntryID + ";");
            WriteSql("set @StartingSpawn2ID = " + _config.Spawn2ID + ";");
            WriteSql("set @StartingGridID = " + _config.GridDBID + ";");
            WriteSql("set @StartingMerchantID = " + _config.MerchantDBID + ";");
            WriteSql("set @BaseDoorID = " + _config.DoorDBID + ";");
            WriteSql("set @StartingGroundSpawnID = " + _config.GroundSpawnDBID + ";");
            WriteSql("set @StartingObjectID = " + _config.ObjectDBID + ";");
            WriteSql("--");
            WriteSql("--");

            if (_config.GenerateZone)
                _streamProcessor.GenerateZoneSQL(WriteSql);

            if (_config.GenerateZonePoint)
                _streamProcessor.GenerateZonePointSQL(_config.ZoneName, WriteSql);


            if (_config.GenerateDoors)
            {
                Log("Starting to generate SQL for Doors.");
                _streamProcessor.GenerateDoorsSQL(_config.ZoneName, _config.DoorDBID, _config.SpawnVersion, WriteSql);
                Log("Finished generating SQL for Doors.");
            }

            Log("Starting to generate SQL for Spawns and/or Grids.");

            _streamProcessor.GenerateSpawnSQL(_config.GenerateSpawns, _config.GenerateGrids, _config.GenerateMerchants, _config.ZoneName, _config.ZoneID, _config.SpawnVersion,
                _config.UpdateExistingNPCTypes, _config.UseNPCTypesTint, _config.SpawnNameFilter, _config.CoalesceWaypoints, _config.GenerateInvisibleMen, WriteSql);

            Log("Finished generating SQL for Spawns and/or Grids.");

            if (_config.GenerateGroundSpawns || _config.GenerateObjects)
            {
                Log("Starting to generate SQL for Ground Spawns and/or Objects.");

                _streamProcessor.GenerateObjectSQL(_config.GenerateGroundSpawns, _config.GenerateObjects, _config.SpawnVersion, WriteSql);

                Log("Finished generating SQL for Ground Spawns and/or Objects.");
            }

            SetStatus( "SQL written to " + _fileName);
            SqlStream.Close();
        }
    }
}

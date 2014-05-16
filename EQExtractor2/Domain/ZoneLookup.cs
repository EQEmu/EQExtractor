using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace EQExtractor2.Domain
{
    using System.Linq;

    public class ZoneLookups
    {
        private List<ZoneLookup> _zones = new List<ZoneLookup>();
        private Dictionary<UInt32, string> _zonesByNumber = new Dictionary<UInt32, string>();
        private Dictionary<string, UInt32> _zonesByName = new Dictionary<string, UInt32>();

        [XmlArray("Zones")]
        [XmlArrayItem("Zone")]
        public List<ZoneLookup> Zones
        {
            get
            {
                return _zones;
            }
            set
            {
                _zones = value;                
                _zonesByName=new Dictionary<string, UInt32>();
                _zonesByNumber=new Dictionary<UInt32, string>();
                if (_zones == null) return;
                foreach (var zone in _zones)
                {
                    if (!_zonesByName.ContainsKey(zone.Name))_zonesByName.Add(zone.Name,zone.Number);
                    if (!_zonesByNumber.ContainsKey(zone.Number)) _zonesByNumber.Add(zone.Number, zone.Name);
                }
            }
        }

        public string ZoneNumberToName(UInt32 zoneNumber)
        {
            if (!_zonesByNumber.Any() && _zones.Any())
            {
                foreach (var zone in _zones)
                {
                    if (!_zonesByNumber.ContainsKey(zone.Number)) _zonesByNumber.Add(zone.Number, zone.Name);
                }
            }
            return _zonesByNumber.ContainsKey(zoneNumber) ? _zonesByNumber[zoneNumber] : "UNKNOWNZONE";
        }

        public UInt32 ZoneNameToNumber(string zoneName)
        {
            if (!_zonesByName.Any() && _zones.Any())
            {
                foreach (var zone in _zones)
                {
                    if (!_zonesByName.ContainsKey(zone.Name)) _zonesByName.Add(zone.Name, zone.Number);
                }
            }
            return _zonesByName.ContainsKey(zoneName) ? _zonesByName[zoneName] : 0;
        }

        /// <summary>
        /// Creates an instance from the specified file path.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        public static ZoneLookups Deserialize(string filePath)
        {
            ZoneLookups register = null;
            using (var myReader = new StreamReader(filePath))
            {
                //re-initialize graphic object else we'll just add extra lines!
                var serializer = new XmlSerializer(typeof(ZoneLookups));
                register = (ZoneLookups)serializer.Deserialize(myReader);
                myReader.Close();
            }
            return register;
        }

        public static void Serialize(string filePath, ZoneLookups item)
        {
            var serializer = new XmlSerializer(typeof(ZoneLookups));
            using (var myWriter = new StreamWriter(filePath))
            {
                serializer.Serialize(myWriter, item);
                myWriter.Close();
            }
        }
    }

    [XmlRoot("Zone")]
    public class ZoneLookup
    {
        
        [XmlAttribute]
        public UInt32 Number { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
    }
}

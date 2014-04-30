using System;
using System.Collections.Generic;
using System.IO;
using EQExtractor2.Domain;

namespace EQExtractor2.Decoders
{
    class PatchApr242014Decoder : PatchApril032014Decoder
    {
        public PatchApr242014Decoder()
        {
            Version = "EQ Client Build Date April 24 2014.";

            PatchConfFileName = "patch_Apr24-2014.conf";

            SupportsSQLGeneration = false;
        }
        public override void ExplorePlayerProfile(StreamWriter outputStream, ByteStream buffer, PacketDirection direction)
        {
            return;
        }
    }
}
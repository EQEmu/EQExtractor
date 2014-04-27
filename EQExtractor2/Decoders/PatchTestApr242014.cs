namespace EQExtractor2.Decoders
{
    class PatchTestApr242014Decoder : PatchApril032014Decoder
    {
        public PatchTestApr242014Decoder()
        {
            Version = "EQ Client Build Date Test April 24 2014.";

            PatchConfFileName = "patch_TestApr24-2014.conf";

            SupportsSQLGeneration = false;
        }
    }
}
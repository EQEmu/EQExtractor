namespace EQExtractor2.Decoders
{
    class PatchTestServerJanuary162013Decoder : PatchJanuary162013Decoder
    {
        public PatchTestServerJanuary162013Decoder()
        {
            Version = "EQ Client Build Date Test Server January 16 2013.";

            PatchConfFileName = "patch_TestServer-Jan16-2013.conf";

            SupportsSQLGeneration = false;
        }
    }
}
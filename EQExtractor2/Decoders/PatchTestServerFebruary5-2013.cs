namespace EQExtractor2.Decoders
{
    class PatchTestServerFebruary52013Decoder : PatchJanuary162013Decoder
    {
        public PatchTestServerFebruary52013Decoder()
        {
            Version = "EQ Client Build Date Test Server February 5 2013.";

            PatchConfFileName = "patch_TestServer-Feb5-2013.conf";

            SupportsSQLGeneration = false;
        }
    }
}
namespace EQExtractor2.Decoders
{
    class PatchJuly132012Decoder : PatchJune252012Decoder
    {
        public PatchJuly132012Decoder()
        {
            Version = "EQ Client Build Date July 13 2012.";

            ExpectedPPLength = 33784;

            PPZoneIDOffset = 26452;

            PatchConfFileName = "patch_July13-2012.conf";
        }
    }
}
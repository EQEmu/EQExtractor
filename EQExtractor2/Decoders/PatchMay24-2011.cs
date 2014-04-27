namespace EQExtractor2.Decoders
{
    class PatchMay242011Decoder : PatchMay122011Decoder
    {
        public PatchMay242011Decoder()
        {
            Version = "EQ Client Build Date May 24 2011.";

            ExpectedPPLength = 28856;

            PPZoneIDOffset = 21524;
        }
    }
}
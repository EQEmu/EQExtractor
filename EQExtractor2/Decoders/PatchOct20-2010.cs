namespace EQExtractor2.Decoders
{
    class PatchOct202010Decoder : PatchTestSep222010Decoder
    {
        public PatchOct202010Decoder()
        {
            Version = "EQ Client Build Date October 20 2010.";

            ExpectedPPLength = 27816;

            PPZoneIDOffset = 20484;
        }
    }
}
namespace EQExtractor2.Decoders
{
    class PatchMarch152011Decoder : PatchFeb082011Decoder
    {
        public PatchMarch152011Decoder()
        {
            Version = "EQ Client Build Date March 2011.";

            PatchConfFileName = "patch_March15-2011.conf";

            ExpectedPPLength = 28536;

            PPZoneIDOffset = 21204;
        }
    }
}
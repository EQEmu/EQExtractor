using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EQExtractor2.Domain
{
    static class StandStateMapper
    {
        public static int MapEqStandStateToEmuAnimation(byte input)
        {
            switch (input)
            {
                case 100:
                case 102://mounts
                    return 0;
                case 110:
                    return 1;//sitting
                case 115:
                case 120:
                    return 3; //fd
                default:
                    throw new ArgumentOutOfRangeException("input",string.Format("invalid stand state={0}",input));
            }
        }
    }
}

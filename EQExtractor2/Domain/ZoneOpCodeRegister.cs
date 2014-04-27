using System.Collections.Generic;

namespace EQExtractor2.Domain
{
    public class ZoneOpCodeRegister
    {
        private readonly Dictionary<string, OpcodeItem> _opcodes = new Dictionary<string, OpcodeItem>();

        public Dictionary<string, OpcodeItem> Opcodes
        {
            get { return _opcodes; }
        }
    }
}
using QuickFix.Fields;

namespace Intelligences.FixProtocol.Fields
{
    public class SymbolLookupModeField : IntField
    {
        public const int ANY_INCLUSION = 0;
        public const int SYMBOL_STARTS_WITH = 1;
        public const int DESCRIPTION_STARTS_WITH = 2;
        public const int ANY_STARTS_WITH = 3;
        public const int EXACT_MATCH = 4;

        public SymbolLookupModeField(int value)
            : base(Tags.SymbolLookupModeField, value)
        {
        }

        public SymbolLookupModeField()
            : this(EXACT_MATCH)
        {
        }
    }
}

using QuickFix.Fields;

namespace Intelligences.FixProtocol.Fields
{
    public class MaxRecordsField : IntField
    {
        public MaxRecordsField(int value)
            : base(Tags.MaxRecordsField, value)
        {
        }

        public MaxRecordsField()
            : this(0)
        {
        }
    }
}

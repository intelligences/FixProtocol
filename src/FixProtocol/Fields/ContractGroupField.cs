using QuickFix.Fields;

namespace Intelligences.FixProtocol.Fields
{
    public class ContractGroupField : StringField
    {
        public ContractGroupField(string value)
            : base(Tags.ContractGroupField, value)
        {
        }

        public ContractGroupField()
            : this(string.Empty)
        {
        }
    }
}

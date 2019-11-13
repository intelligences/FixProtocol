using QuickFix.Fields;
namespace Intelligences.FixProtocol.Fields
{
    internal class NoSecurityTypesField : IntField
    {
        public NoSecurityTypesField(int value)
            : base(Tags.NoSecurityTypesField, value)
        {
        }

        public NoSecurityTypesField()
            : this(0)
        {
        }
    }
}

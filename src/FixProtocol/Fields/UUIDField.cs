using QuickFix.Fields;

namespace Intelligences.FixProtocol.Fields
{
    internal class UUIDField : StringField
    {
        public UUIDField(string value)
            : base(Tags.UUIDField, value)
        {
        }

        public UUIDField()
            : this(string.Empty)
        {
        }
    }
}

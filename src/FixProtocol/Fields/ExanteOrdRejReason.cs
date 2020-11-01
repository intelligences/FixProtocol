using QuickFix.Fields;
namespace Intelligences.FixProtocol.Fields
{
    internal class ExanteOrdRejReason : StringField
    {
        public ExanteOrdRejReason(string value) : base(Tags.ExanteOrdRejReason, value)
        {
        }

        public ExanteOrdRejReason() : this(string.Empty)
        {
        }
    }
}

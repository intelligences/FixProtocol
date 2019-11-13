using QuickFix.Fields;

namespace Intelligences.FixProtocol.Fields
{
    internal class AccSumReqID : StringField
    {
        public AccSumReqID(string value) : base(Tags.AccSumReqID, value)
        {
        }

        public AccSumReqID() : this(string.Empty)
        {
        }
    }
}

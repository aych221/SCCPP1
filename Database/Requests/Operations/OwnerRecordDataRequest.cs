using SCCPP1.User;

namespace SCCPP1.Database.Requests.Operations
{
    public abstract class OwnerRecordDataRequest : RecordDataRequest
    {

        public OwnerRecordDataRequest(Account owner)
            : base(owner.Data)
        {
        }
    }
}

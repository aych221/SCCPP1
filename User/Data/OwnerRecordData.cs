using Microsoft.EntityFrameworkCore.Diagnostics;

namespace SCCPP1.User.Data
{
    public abstract class OwnerRecordData : RecordData
    {
        public Account Owner { get; set; }


        public OwnerRecordData(Account owner) : this(owner, -1)
        {

        }

        public OwnerRecordData(Account owner, int id) : base(id)
        {
            this.Owner = owner;
        }

    }
}

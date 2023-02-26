namespace SCCPP1.User.Data
{
    public class AccountRecordData
    {
        public Account Owner { get; set; }

        public int RecordID { get; set; }


        public AccountRecordData(Account owner, int id = -1)
        {
            this.Owner = owner;
            this.RecordID = id;
        }
    }
}

namespace SCCPP1.User.Data
{
    public class WorkData : AccountRecordData
    {
        public string Employer { get; set; }

        public int EmployerID { get; set; }

        public Location Location { get; set; }

        public string JobTitle { get; set; }

        public int JobTitleID { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        public string Description { get; set; }


        public WorkData(Account owner, int id) : base(owner, id)
        {

        }
    }
}

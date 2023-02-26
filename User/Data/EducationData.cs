namespace SCCPP1.User.Data
{
    public class EducationData : AccountRecordData
    {

        public string EducationType { get; set; }
        public int EducationTypeID { get; set; }

        public string Institution { get; set; }
        public int InstitutionID { get; set; }

        public Location Location { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        public string Description { get; set; }


        public EducationData(Account owner, int id) : base(owner, id)
        {

        }

    }
}

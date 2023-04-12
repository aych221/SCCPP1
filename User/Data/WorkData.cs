namespace SCCPP1.User.Data
{
    public class WorkData : OwnerRecordData
    {

        private string employer;
        public string Employer
        {
            get { return employer; }
            set { SetField(ref employer, value); }
        }
        public int EmployerID { get; set; }


        private string jobTitle;
        public string JobTitle
        {
            get { return jobTitle; }
            set { SetField(ref jobTitle, value); }
        }
        public int JobTitleID { get; set; }


        private string description;
        public string Description
        {
            get { return description; }
            set { SetField(ref description, value); }
        }


        private Location location;
        public Location Location
        {
            get { return location; }
            set { SetField(ref location, value); }
        }


        private DateOnly startDate;
        public DateOnly StartDate
        {
            get { return startDate; }
            set { SetField(ref startDate, value); }
        }


        private DateOnly endDate;
        public DateOnly EndDate
        {
            get { return endDate; }
            set { SetField(ref endDate, value); }
        }




        public WorkData(Account owner, int recordID = -1) : base(owner, recordID)
        {

        }

        public WorkData(Account owner, int recordID, string employer, int employerID, string jobTitle, int jobTitleID, string description, Location location, DateOnly startDate, DateOnly endDate) : this(owner, recordID)
        {
            this.Employer = employer;
            this.EmployerID = employerID;
            this.Location = location;
            this.JobTitle = jobTitle;
            this.JobTitleID = jobTitleID;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.Description = description;

            this.IsUpdated = true;
            
        }


        //for user creation
        public WorkData(Account owner, string employer, string jobTitle, string description, Location location, DateOnly startDate, DateOnly endDate) : 
            this(owner, -1, employer, -1, jobTitle, -1, description, location, startDate, endDate)
        {
            this.NeedsSave = true;
        }


        //for loading from db
        public WorkData(Account owner, int recordID, int employerID, int jobTitleID, string description, Location location, DateOnly startDate, DateOnly endDate) :
            this(owner, recordID, null, employerID, null, jobTitleID, description, location, startDate, endDate)
        {
            this.NeedsSave = false;
            //maybe use IsUpdated = false flag to let DB know we need null values filled?
        }


        public override bool Save()
        {
            //save resources if we don't need to save.
            /*if (!this.NeedsSave)
                return true;*/

            return NeedsSave = !(IsUpdated = DatabaseConnector.SaveWorkHistory(this));
        }


        /// <summary>
        /// Deletes the profile record.
        /// </summary>
        /// <returns>true if record was removed from database, false otherwise.</returns>
        public override bool Delete()
        {
            if (!Remove)
                return true;

            //TODO put database remove method
            //NeedsSave = !(IsUpdated
            return true;
        }

    }
}

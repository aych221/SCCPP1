using System.Security.Cryptography;

namespace SCCPP1.User.Data
{
    public class EducationData : OwnerRecordData
    {

        private string institution;
        public string Institution
        { 
            get { return institution; } 
            set
            { SetField(ref institution, value); } 
        }

        public int InstitutionID { get; set; }

        private string educationType;
        public string EducationType
        {
            get { return educationType; }
            set { SetField(ref educationType, value); }
        }

        public int EducationTypeID { get; set; }

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


        public EducationData(Account owner, int recordID = -1) : base(owner, recordID)
        {

        }


        public EducationData(Account owner, int recordID, string institution, int institutionID, string educationType, int educationTypeID, string description, Location location, DateOnly startDate, DateOnly endDate) : this(owner, recordID)
        {
            this.Institution = institution;
            this.InstitutionID = institutionID;
            this.EducationType = educationType;
            this.EducationTypeID = educationTypeID;
            this.Description = description;
            this.Location = location;
            this.StartDate = startDate;
            this.EndDate = endDate;

            this.IsUpdated = true;
        }

        //for creating from user input
        public EducationData(Account owner, string institution, string educationType, string description, Location location, DateOnly startDate, DateOnly endDate) : 
            this(owner, -1, institution, -1, educationType, -1, description, location, startDate, endDate)
        {
            this.NeedsSave = true;
        }

        //for loading from db
        public EducationData(Account owner, int recordID, int institutionID, int educationTypeID, string description, Location location, DateOnly startDate, DateOnly endDate) :
            this(owner, recordID, null, institutionID, null, educationTypeID, description, location, startDate, endDate)
        {
            this.NeedsSave = false;
            //maybe use IsUpdated = false flag to let DB know we need null values filled?
        }


        public override bool Save()
        {
            //save resources if we don't need to save.
            /*if (!this.NeedsSave)
                return true;*/

            return NeedsSave = !(IsUpdated = DatabaseConnector.SaveEducationHistory(this));
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

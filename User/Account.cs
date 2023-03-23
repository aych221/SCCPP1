using SCCPP1.Session;
using SCCPP1.User.Data;

namespace SCCPP1.User
{
    public class Account : RecordData
    {

        protected readonly SessionData Data;


        //0 = admin, 1 = normal user
        private int role;
        public int Role
        {
            get { return role; }
            set { SetField(ref role, value); }
        }


        //Name stored
        private string name;
        public string Name
        {
            get { return name; }
            set { SetField(ref name, value); }
        }

        private string firstName;
        public string FirstName
        {
            get { return firstName; }
            set { SetField(ref firstName, value); }
        }

        private string lastName;
        public string LastName
        {
            get { return lastName; }
            set { SetField(ref lastName, value); }
        }

        private string? middleName;
        public string? MiddleName
        {
            get { return middleName; }
            set { SetField(ref middleName, value); }
        }


        //Email stored (may not want to use signed on E-mail)
        private string emailAddress;
        public string EmailAddress
        {
            get { return emailAddress; }
            set { SetField(ref emailAddress, value); }
        }

        private long phoneNumber;
        public long PhoneNumber
        {
            get { return phoneNumber; }
            set { SetField(ref phoneNumber, value); }
        }


        private string streetAddress;
        public string StreetAddress
        {
            get { return streetAddress; }
            set { SetField(ref streetAddress, value); }
        }


        private Location location;
        public Location Location
        {
            get { return location; }
            set { SetField(ref location, value); }
        }


        private string introNarrative;
        public string IntroNarrative
        {
            get { return introNarrative; }
            set { SetField(ref introNarrative, value); }
        }


        private int mainProfileID;
        public int MainProfileID
        {
            get { return mainProfileID; }
            set { SetField(ref mainProfileID, value); }
        }


        //may want to change these to dictionaries
        public List<SkillData> Skills { get; set; }

        public List<EducationData> EducationHistory { get; set; }

        public List<WorkData> WorkHistory { get; set; }



        /// <summary>
        /// Are they a returning user?
        /// </summary>
        public bool IsReturning;



        public Account(SessionData sessionData, bool isReturning) : base()
        {
            this.Data = sessionData;
            this.IsReturning= isReturning;

            this.EmailAddress = Data.GetUsersEmail();
            this.Name = Data.GetUsersName();

        }


        #region Add/Update data methods
        public void UpdateData(string firstName, string middleName, string lastName, string emailAddress, long phoneNumber, string introNarrative)
        {
            FirstName = firstName;
            MiddleName = middleName;
            LastName = lastName;
            Name = Utilities.ToFullName(firstName, middleName, lastName);
            EmailAddress = emailAddress;
            PhoneNumber = phoneNumber;
            IntroNarrative = introNarrative;
        }


        public void UpdateData(string name, string emailAddress, long phoneNumber, string introNarrative)
        {
            Name = name;
            string[] names = Utilities.SplitFullName(Name);
            UpdateData(names[1], names[2], names[0], emailAddress, phoneNumber, introNarrative);
        }


        public void AddSkills(params string[] skillNames)
        {
            if (skillNames == null || skillNames.Length == 0)
                return;

            foreach (string skillName in skillNames)
                Skills.Add(new SkillData(this, skillName, -1));

            NeedsSave = true;
        }

        
        public void RemoveSkills(params string[] skillNames)
        {
            //TODO, need to make DB methods to remove colleague records that are requested
        }


        public void AddEducation(string institution, string educationType, string description, Location location, DateOnly startDate, DateOnly endDate)
        {
            EducationHistory.Add(new EducationData(this, institution, educationType, description, location, startDate, endDate));
        }


        public void AddWork(string employer, string jobTitle, string description, Location location, DateOnly startDate, DateOnly endDate)
        {
            WorkHistory.Add(new WorkData(this, employer, jobTitle, description, location, startDate, endDate));
        }

        #endregion





        public string GetUsername()
        {
            return Data.Username;
        }


        public bool Load()
        {
            return DatabaseConnector.GetUser(this);
        }

        public bool LoadSkills()
        {
            return DatabaseConnector.LoadColleagueSkills(this);
        }

        public bool LoadSkills(out Dictionary<int, SkillData> skills)
        {
            return DatabaseConnector.LoadColleagueSkills(this, out skills);
        }

        public bool LoadEducationHistory()
        {
            return DatabaseConnector.LoadColleagueEducationHistory1(this);
        }

        public bool LoadEducationHistory(out Dictionary<int, EducationData> educationHistory)
        {
            return DatabaseConnector.LoadColleagueEducationHistory1(this, out educationHistory);
        }


        public bool LoadWorkHistory()
        {
            return DatabaseConnector.LoadColleagueWorkHistory1(this);
        }

        public bool LoadWorkHistory(out Dictionary<int, WorkData> workHistory)
        {
            return DatabaseConnector.LoadColleagueWorkHistory1(this, out workHistory);
        }


        /// <summary>
        /// Saves the user's direct profile information, does not save associated data.
        /// </summary>
        /// <returns>true if changes are saved in database, false otherwise.</returns>
        public override bool Save()
        {
            if (Remove)
            {
                //TODO put database remove method
                return true;
            }
            else if (!NeedsSave)
                return true;

            return NeedsSave = !(IsUpdated = DatabaseConnector.SaveUser(this));
        }


        /// <summary>
        /// Saves everything for the user. All associated data is saved to the database.
        /// </summary>
        /// <returns>true if changes are saved in the database, false otherwise.</returns>
        public bool SaveAll()
        {
            bool failed = false;

            //Save user first
            Save();

            //Batch save skills
            foreach (SkillData sd in Skills)
                if (!sd.Save())
                    failed = true;

            //Batch save education history
            foreach (EducationData ed in EducationHistory)
                if (!ed.Save())
                    failed = true;

            //Batch save work history
            foreach (WorkData wd in WorkHistory)
                if (!wd.Save())
                    failed = true;

            return failed;
        }


    }
}

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

        public List<ProfileData> Profiles { get; set; }



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

        /// <summary>
        /// Updates the basic account data for this colleague. Updates NeedsSave and IsUpdated flags.
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="middleName"></param>
        /// <param name="lastName"></param>
        /// <param name="emailAddress"></param>
        /// <param name="phoneNumber"></param>
        /// <param name="introNarrative"></param>
        public void UpdateData(string firstName, string middleName, string lastName, string emailAddress, long phoneNumber, string introNarrative)
        {
            FirstName = firstName;
            MiddleName = middleName;
            LastName = lastName;
            Name = Utilities.ToFullName(firstName, middleName, lastName);
            EmailAddress = emailAddress;
            PhoneNumber = phoneNumber;
            IntroNarrative = introNarrative;
            NeedsSave = IsUpdated = true;
        }


        /// <summary>
        /// Updates the basic account data for this colleague. Updates NeedsSave and IsUpdated flags.
        /// </summary>
        /// <param name="name">Passed in as "LastName, FirstName [MiddleName]"</param>
        /// <param name="emailAddress"></param>
        /// <param name="phoneNumber"></param>
        /// <param name="introNarrative"></param>
        public void UpdateData(string name, string emailAddress, long phoneNumber, string introNarrative)
        {
            Name = name;
            string[] names = Utilities.SplitFullName(Name);
            UpdateData(names[1], names[2], names[0], emailAddress, phoneNumber, introNarrative);
        }


        /// <summary>
        /// Adds a list of skills to the account.
        /// </summary>
        /// <param name="skillNames"></param>
        public void AddSkills(params string[] skillNames)
        {
            if (skillNames == null || skillNames.Length == 0)
                return;

            foreach (string skillName in skillNames)
                Skills.Add(new SkillData(this, skillName, -1));

            NeedsSave = IsUpdated = true;
        }


        /// <summary>
        /// Adds a list of skills to the account for the specified skill category.
        /// </summary>
        /// <param name="skillCategory">Category of skill</param>
        /// <param name="skillNames"></param>
        public void AddSkills(string skillCategory, params string[] skillNames)
        {
            if (skillNames == null || skillNames.Length == 0)
                return;

            foreach (string skillName in skillNames)
                Skills.Add(new SkillData(this, skillCategory, skillName, -1));

            NeedsSave = IsUpdated = true;
        }


        public void RemoveSkills(params string[] skillNames)
        {
            //TODO, need to make DB methods to remove colleague records that are requested
        }


        public void AddEducation(string institution, string educationType, string description, Location location, DateOnly startDate, DateOnly endDate)
        {
            EducationHistory.Add(new EducationData(this, institution, educationType, description, location, startDate, endDate));
            NeedsSave = IsUpdated = true;
        }


        public void AddWork(string employer, string jobTitle, string description, Location location, DateOnly startDate, DateOnly endDate)
        {
            WorkHistory.Add(new WorkData(this, employer, jobTitle, description, location, startDate, endDate));
            NeedsSave = IsUpdated = true;
        }


        /// <summary>
        /// Saves current skill data and attempts to fetch the record
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SkillData EditSkillData(int id)
        {
            //save skills
            //need to find better way to reference specific objects, might use UUID upon object creation.
            SaveSkills();

            //need to make better way that isn't O(n). use dictionaries later
            return Skills.Find(x => x.RecordID == id);
        }

        public EducationData EditEducationData(int id)
        {
            //save education
            SaveEducationHistory();

            return EducationHistory.Find(x => x.RecordID == id);
        }

        public WorkData EditWorkData(int id)
        {
            //save work
            SaveWorkHistory();

            return WorkHistory.Find(x => x.RecordID == id);
        }
        #endregion





        public string GetUsername()
        {
            return Data.Username;
        }

        /// <summary>
        /// Attempts to fetch skill record
        /// </summary>
        /// <param name="id"></param>
        /// <returns>SkillData if object is found, null otherwise</returns>
        public SkillData? GetSkillData(int id)
        {
            //need to make better way that isn't O(n). use dictionaries later
            return Skills.Find(x => x.RecordID == id);
        }

        /// <summary>
        /// Attempts to fetch education history record
        /// </summary>
        /// <param name="id"></param>
        /// <returns>EducationData if object is found, null otherwise</returns>
        public EducationData? GetEducationData(int id)
        {
            return EducationHistory.Find(x => x.RecordID == id);
        }


        /// <summary>
        /// Attempts to fetch work history record
        /// </summary>
        /// <param name="id"></param>
        /// <returns>WorkData if object is found, null otherwise</returns>
        public WorkData? GetWorkData(int id)
        {
            return WorkHistory.Find(x => x.RecordID == id);
        }


        public bool Load()
        {
            return DatabaseConnector.GetUser(this);
        }

        public bool LoadSkills()
        {
            return DatabaseConnector.LoadColleagueSkills1(this);
        }

        public bool LoadSkills(out Dictionary<int, SkillData> skills)
        {
            return DatabaseConnector.LoadColleagueSkills1(this, out skills);
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


        #region Profile methods

        /// <summary>
        /// Creates a new profile object, which also saves all data in the account to the database, excluding other profiles.
        /// </summary>
        /// <param name="title"></param>
        /// <returns>A new profile object for the user</returns>
        public ProfileData CreateProfile(string title)
        {
            //may not need in the future, considered a "heavy" call.
            SaveAll();
            ProfileData pd = new ProfileData(this);
            pd.Title = title;
            pd.LoadData();

            return pd;
        }

        public ProfileData? GetProfile(int id)
        {
            return Profiles.Find(x => x.RecordID == id);
        }

        #endregion


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
        /// Deletes the user's data and all records associated with it.
        /// </summary>
        /// <returns>true if records were removed from database, false otherwise.</returns>
        protected override bool Delete()
        {
            if (!Remove)
                return true;

            //TODO put database remove method
            //NeedsSave = !(IsUpdated
            return true;
        }


        /// <summary>
        /// Saves everything for the user. All associated data is saved to the database.
        /// </summary>
        /// <returns>true if changes are saved in the database, false otherwise.</returns>
        public bool SaveAll()
        {

            //TODO add list of saves
            //List<SkillData> skillSaves = new List<SkillData>();

            return Save() && SaveSkills() && SaveEducationHistory() && SaveWorkHistory(); 
        }


        /// <summary>
        /// Saves all Skill data for the user.
        /// </summary>
        /// <returns>true if changes are saved in the database, false otherwise.</returns>
        protected bool SaveSkills()
        {
            bool failed = false;

            foreach (SkillData sd in Skills)
                if (!sd.Save())
                    failed = true;

            return failed;
        }


        /// <summary>
        /// Saves all Education data for the user.
        /// </summary>
        /// <returns>true if changes are saved in the database, false otherwise.</returns>
        protected bool SaveEducationHistory()
        {
            bool failed = false;

            foreach (EducationData ed in EducationHistory)
                if (!ed.Save())
                    failed = true;

            return failed;
        }



        /// <summary>
        /// Saves all Work data for the user.
        /// </summary>
        /// <returns>true if changes are saved in the database, false otherwise.</returns>
        protected bool SaveWorkHistory()
        {
            bool failed = false;

            foreach (WorkData wd in WorkHistory)
                if (!wd.Save())
                    failed = true;

            return failed;
        }

    }
}

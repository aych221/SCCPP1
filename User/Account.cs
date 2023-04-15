using SCCPP1.Models;
using SCCPP1.Session;
using SCCPP1.User.Data;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

namespace SCCPP1.User
{
    public class Account : RecordData
    {

        protected readonly SessionData Data;


        private ProfileData chosenProfile;
        public void ChooseProfile(ProfileData pd)
        {
            chosenProfile = pd;
        }
        public ProfileData ChosenProfile()
        {
            return chosenProfile;
        }
        #region Account Data properties
        //TODO: change all properties to use protected sets
        //0 = admin, 1 = normal user
        private int _role;
        public int Role
        {
            get { return _role; }
            set { SetField(ref _role, value); }
        }


        //Name stored
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value); }
        }

        private string _firstName;
        public string FirstName
        {
            get { return _firstName; }
            set { SetField(ref _firstName, value); }
        }

        private string _lastName;
        public string LastName
        {
            get { return _lastName; }
            set { SetField(ref _lastName, value); }
        }


        private string? _middleName;
        public string? MiddleName
        {
            get { return _middleName; }
            set { SetField(ref _middleName, value); }
        }


        //Email stored (may not want to use signed on E-mail)
        private string _emailAddress;
        public string EmailAddress
        {
            get { return _emailAddress; }
            set { SetField(ref _emailAddress, value); }
        }

        private long _phoneNumber;
        public long PhoneNumber
        {
            get { return _phoneNumber; }
            set { SetField(ref _phoneNumber, value); }
        }


        private string _streetAddress;
        public string StreetAddress
        {
            get { return _streetAddress; }
            set { SetField(ref _streetAddress, value); }
        }


        private Location _location;
        public Location Location
        {
            get { return _location; }
            set { SetField(ref _location, value); }
        }


        private string _introNarrative;
        public string IntroNarrative
        {
            get { return _introNarrative; }
            set { SetField(ref _introNarrative, value); }
        }


        private int _mainProfileID;
        public int MainProfileID
        {
            get { return _mainProfileID; }
            set { SetField(ref _mainProfileID, value); }
        }

        #endregion



        //may want to change these to dictionaries
        [Obsolete("Use SavedSkills instead", false)]
        public List<SkillData> Skills { get; set; }


        [Obsolete("Use SavedEducation instead", false)]
        public List<EducationData> EducationHistory { get; set; }


        [Obsolete("Use SavedWorkHistory instead", false)]
        public List<WorkData> WorkHistory { get; set; }


        [Obsolete("Use SavedProfiles instead", false)]
        public List<ProfileData> Profiles { get; set; }



        #region Unsaved Data Lists
        private List<SkillData> _unsavedSkills;

        protected List<SkillData> UnsavedSkills
        {
            get { return _unsavedSkills; }
            set { SetField(ref _unsavedSkills, value); }
        }



        private List<EducationData> _unsavedEducationHistory;

        protected List<EducationData> UnsavedEducationHistory
        {
            get { return _unsavedEducationHistory; }
            set { SetField(ref _unsavedEducationHistory, value); }
        }



        private List<CertificationData> _unsavedCertificates;

        protected List<CertificationData> UnsavedCertifications
        {
            get { return _unsavedCertificates; }
            set { SetField(ref _unsavedCertificates, value); }
        }



        private List<WorkData> _unsavedWorkHistory;

        protected List<WorkData> UnsavedWorkHistory
        {
            get { return _unsavedWorkHistory; }
            set { SetField(ref _unsavedWorkHistory, value); }
        }



        private List<ProfileData> _unsavedProfiles;
        
        protected List<ProfileData> UnsavedProfiles
        {
            get { return _unsavedProfiles; }
            set { SetField(ref _unsavedProfiles, value); }
        }
        #endregion


        #region Saved Data Dictionaries
        private ReadOnlyDictionary<int, SkillData> _savedSkills;

        public ReadOnlyDictionary<int, SkillData> SavedSkills
        {
            get { return _savedSkills; }
            protected set { SetField(ref _savedSkills, value); }
        }



        private ReadOnlyDictionary<int, EducationData> _savedEducationHistory;

        public ReadOnlyDictionary<int, EducationData> SavedEducationHistory
        {
            get { return _savedEducationHistory; }
            protected set { SetField(ref _savedEducationHistory, value); }
        }



        private ReadOnlyDictionary<int, CertificationData> _savedCertifications;

        public ReadOnlyDictionary<int, CertificationData> SavedCertifications
        {
            get { return _savedCertifications; }
            protected set { SetField(ref _savedCertifications, value); }
        }



        private ReadOnlyDictionary<int, WorkData> _savedWorkHistory;

        public ReadOnlyDictionary<int, WorkData> SavedWorkHistory
        {
            get { return _savedWorkHistory; }
            protected set { SetField(ref _savedWorkHistory, value); }
        }


        private ReadOnlyDictionary<int, ProfileData> _savedProfiles;

        public ReadOnlyDictionary<int, ProfileData> SavedProfiles
        {
            get { return _savedProfiles; }
            protected set { SetField(ref _savedProfiles, value); }
        }
        #endregion





        /// <summary>
        /// Are they a returning user?
        /// </summary>
        public bool IsReturning { get; set; }



        public Account(SessionData sessionData, bool isReturning) : base()
        {
            this.Data = sessionData;
            this.IsReturning = isReturning;

            this.EmailAddress = Data.GetUsersEmail();
            this.Name = Data.GetUsersName();

            UnsavedSkills = new List<SkillData>();
            UnsavedEducationHistory = new List<EducationData>();
            UnsavedCertifications = new List<CertificationData>();
            UnsavedWorkHistory = new List<WorkData>();
            UnsavedProfiles = new List<ProfileData>();

            SavedSkills = new ReadOnlyDictionary<int, SkillData>(new Dictionary<int, SkillData>());
            SavedCertifications = new ReadOnlyDictionary<int, CertificationData>(new Dictionary<int, CertificationData>());
            SavedEducationHistory = new ReadOnlyDictionary<int, EducationData>(new Dictionary<int, EducationData>());
            SavedWorkHistory = new ReadOnlyDictionary<int, WorkData>(new Dictionary<int, WorkData>());
            SavedProfiles = new ReadOnlyDictionary<int, ProfileData>(new Dictionary<int, ProfileData>());
        }

        public Account(string username)
        {
            this.Data = null;
            this.IsReturning = false;

            this.Name = username;

            UnsavedSkills = new List<SkillData>();
            UnsavedEducationHistory = new List<EducationData>();
            UnsavedCertifications = new List<CertificationData>();
            UnsavedWorkHistory = new List<WorkData>();
            UnsavedProfiles = new List<ProfileData>();

            SavedSkills = new ReadOnlyDictionary<int, SkillData>(new Dictionary<int, SkillData>());
            SavedCertifications = new ReadOnlyDictionary<int, CertificationData>(new Dictionary<int, CertificationData>());
            SavedEducationHistory = new ReadOnlyDictionary<int, EducationData>(new Dictionary<int, EducationData>());
            SavedWorkHistory = new ReadOnlyDictionary<int, WorkData>(new Dictionary<int, WorkData>());
            SavedProfiles = new ReadOnlyDictionary<int, ProfileData>(new Dictionary<int, ProfileData>());
        }


        #region Add/Remove/Edit data methods

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
            {
                //Skills.Add(new SkillData(this, skillName, -1));
                UnsavedSkills.Add(new SkillData(this, skillName, -1));
            }

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
            {
                //Skills.Add(new SkillData(this, skillCategory, skillName, -1)); //later will need to remove this
                UnsavedSkills.Add(new SkillData(this, skillCategory, skillName, -1));
            }

            NeedsSave = IsUpdated = true;
        }


        public void RemoveSkills(params string[] skillNames)
        {
            //TODO, need to make DB methods to remove colleague records that are requested
        }


        public void AddEducation(string institution, string degreeType, string field, Location? location, DateOnly? startDate, DateOnly? endDate)
        {
            //EducationHistory.Add(new EducationData(this, institution, degreeType, field, new Location(), new DateOnly(), new DateOnly()));
            UnsavedEducationHistory.Add(new EducationData(this, institution, degreeType, field, location.GetValueOrDefault(), startDate.GetValueOrDefault(), endDate.GetValueOrDefault()));
            NeedsSave = IsUpdated = true;
        }


        public void AddEducation(string institution, string degreeType, string field)
        {
            AddEducation(institution, degreeType, field, null, null, null);
        }



        public void AddCertification(string institution, string certificationType, string description, Location? location, DateOnly? startDate, DateOnly? endDate)
        {

            UnsavedCertifications.Add(new CertificationData(this, institution, certificationType, description, location.GetValueOrDefault(), startDate.GetValueOrDefault(), endDate.GetValueOrDefault()));
            NeedsSave = IsUpdated = true;
        }


        public void AddCertification(string institution, string certificationType, DateOnly? startDate, DateOnly? endDate)
        {
            AddCertification(institution, certificationType, null, null, startDate.GetValueOrDefault(), endDate.GetValueOrDefault());
        }


        public void AddWork(string employer, string jobTitle, string description, Location? location, DateOnly? startDate, DateOnly? endDate)
        {
            //WorkHistory.Add(new WorkData(this, employer, jobTitle, description, location, startDate, endDate));
            UnsavedWorkHistory.Add(new WorkData(this, employer, jobTitle, description, location.GetValueOrDefault(), startDate.GetValueOrDefault(), endDate.GetValueOrDefault()));
            NeedsSave = IsUpdated = true;
        }


        [Obsolete("Use GetSkillData() instead.")]
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

        [Obsolete("Use GetEducationData() instead.")]
        public EducationData EditEducationData(int id)
        {
            //save education
            SaveEducationHistory();

            return EducationHistory.Find(x => x.RecordID == id);
        }

        [Obsolete("Use GetWorkData() instead.")]
        public WorkData EditWorkData(int id)
        {
            //save work
            SaveWorkHistory();

            return WorkHistory.Find(x => x.RecordID == id);
        }
        #endregion




        #region Get Data methods

        /// <summary>
        /// Gets the username of the account.
        /// </summary>
        /// <returns>The SessionData.Username</returns>
        public string GetUsername()
        {
            if (Data != null)
                return Data.Username;
            return Name;
        }


        /// <summary>
        /// Attempts to fetch a saved SkillData record.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>SkillData if object is found, null otherwise</returns>
        public SkillData? GetSkillData(int id)
        {
            return SavedSkills[id];
        }


        /// <summary>
        /// Attempts to fetch a saved EducationData record.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>EducationData if object is found, null otherwise</returns>
        public EducationData? GetEducationData(int id)
        {
            return SavedEducationHistory[id];
        }


        /// <summary>
        /// Attempts to fetch a saved WorkData record.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>WorkData if object is found, null otherwise</returns>
        public WorkData? GetWorkData(int id)
        {
            return SavedWorkHistory[id];
        }


        /// <summary>
        /// Attempts to fetch a saved ProfileData record.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>ProfuleData if object is found, null otherwise</returns>
        public ProfileData? GetProfileData(int id)
        {
            return SavedProfiles[id];
        }


        /// <summary>
        /// Attempts to fetch a saved CertificationData record.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>CertificationData if object is found, null otherwise</returns>
        public CertificationData? GetCertificationData(int id)
        {
            return SavedCertifications[id];
        }
        #endregion




        #region Profile methods

        /// <summary>
        /// Creates a new profile object, which also saves all data in the account to the database, excluding other profiles.
        /// </summary>
        /// <param name="title"></param>
        /// <returns>A new profile object for the user</returns>
        public ProfileData CreateProfile(string title)
        {
            //might want to load main profile as default.



            //may not need in the future, considered an expensive call.
            PersistAll();

            //might want to create these hashsets in LoadAll() method to reduce memory and computations
            ProfileData pd = new ProfileData(this, title);

                //new HashSet<int>(SavedSkills.Keys),
                //new HashSet<int>(SavedEducationHistory.Keys),
                //new HashSet<int>(SavedCertifications.Keys),
                //new HashSet<int>(SavedWorkHistory.Keys),
                //""
                //);
            UnsavedProfiles.Add(pd);
            return pd;
        }

        #endregion




        #region Load methods

        [Obsolete("Use PersistAll() instead.")]
        public void LoadAll()
        {
            Load();

            Dictionary<int, SkillData> savedSkills;
            Dictionary<int, EducationData> savedEducationHistory;
            Dictionary<int, WorkData> savedWorkHistory;
            Dictionary<int, ProfileData> savedProfiles;

            LoadSkills(out savedSkills);
            LoadEducationHistory(out savedEducationHistory);
            LoadWorkHistory(out savedWorkHistory);
            LoadProfiles(out savedProfiles);

            SavedSkills = new ReadOnlyDictionary<int, SkillData>(savedSkills);
            SavedEducationHistory = new ReadOnlyDictionary<int, EducationData>(savedEducationHistory);
            SavedWorkHistory = new ReadOnlyDictionary<int, WorkData>(savedWorkHistory);
            SavedProfiles = new ReadOnlyDictionary<int, ProfileData>(savedProfiles);
        }


        [Obsolete("Use Persist() instead.")]
        public bool Load()
        {
            return DatabaseConnector.GetUser(this);
        }

        [Obsolete("Use PersistSkills() instead.")]
        public bool LoadSkills()
        {
            return DatabaseConnector.LoadColleagueSkills1(this);
        }

        [Obsolete("Use PersistSkills() and the SavedSkills property instead.")]
        public bool LoadSkills(out Dictionary<int, SkillData> skills)
        {
            return DatabaseConnector.LoadColleagueSkills1(this, out skills);
        }

        [Obsolete("Use PersistEducationHistory() instead.")]
        public bool LoadEducationHistory()
        {
            return DatabaseConnector.LoadColleagueEducationHistory1(this);
        }

        [Obsolete("Use PersistEducationHistory() and the SavedEducationHistory property instead.")]
        public bool LoadEducationHistory(out Dictionary<int, EducationData> educationHistory)
        {
            return DatabaseConnector.LoadColleagueEducationHistory1(this, out educationHistory);
        }


        [Obsolete("Use PersistWorkHistory() instead.")]
        public bool LoadWorkHistory()
        {
            return DatabaseConnector.LoadColleagueWorkHistory1(this);
        }

        [Obsolete("Use PersistWorkHistory() and the SavedWorkHistory property instead.")]
        public bool LoadWorkHistory(out Dictionary<int, WorkData> workHistory)
        {
            return DatabaseConnector.LoadColleagueWorkHistory1(this, out workHistory);
        }

        [Obsolete("Use PersistProfiles() instead.")]
        public bool LoadProfiles()
        {
            return DatabaseConnector.LoadColleageProfiles(this);
        }

        [Obsolete("Use PersistProfiles() and the SavedProfiles property instead.")]
        public bool LoadProfiles(out Dictionary<int, ProfileData> profiles)
        {
            return DatabaseConnector.LoadColleageProfiles(this, out profiles);
        }
        #endregion



        #region Save methods

        /// <summary>
        /// Saves the user's direct profile information, does not save associated data.
        /// </summary>
        /// <returns>true if changes are saved in database, false otherwise.</returns>
        public override bool Save()
        {
            /*if (!NeedsSave)
                return true;*/

            return NeedsSave = !(IsUpdated = DatabaseConnector.SaveUser(this));
        }


        [Obsolete("Use PersistAll() instead.")]
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


        [Obsolete("Use PersistSkills() instead.")]
        /// <summary>
        /// Saves all Skill data for the user.
        /// </summary>
        /// <returns>true if changes are saved in the database, false otherwise.</returns>
        protected bool SaveSkills()
        {
            bool failed = false;

            //Save the saved skills
            foreach (SkillData sd in Skills)
                if (!sd.Save())
                    failed = true;

            return failed;
        }


        [Obsolete("Use PersistEducationHistory() instead.")]
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



        [Obsolete("Use PersistWorkHistory() instead.")]
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


        [Obsolete("Use PersistProfiles() instead.")]
        protected bool SaveProfiles()
        {
            bool failed = false;
            foreach (ProfileData pd in Profiles)
                if (!pd.Save())
                    failed = true;
            return failed;
        }

        #endregion



        #region Delete methods
        /// <summary>
        /// Deletes the user's data and all records associated with it.
        /// </summary>
        /// <returns>true if records were removed from database, false otherwise.</returns>
        public override bool Delete()
        {
            if (!Remove)
                return true;

            //TODO put database remove method
            //NeedsSave = !(IsUpdated
            return true;
        }

        #endregion


        #region Persist methods
        /// <summary>
        /// This method will save, update, or delete and reload data to ensure all data is updated in the database.
        /// </summary>
        /// <returns>true if the operation was successful, false otherwise.</returns>
        public bool PersistAll()
        {
            return Persist() && PersistSkills() && PersistCertifications() && PersistEducationHistory() && PersistWorkHistory() && PersistProfiles();
        }


        /// <summary>
        /// This method will save, update, or delete and reload data to ensure all account data is updated in the database.
        /// </summary>
        /// <returns>true if the operation was successful, false otherwise.</returns>
        public bool Persist()
        {
            bool success = true;

            //save, update, or delete account
            if (Remove)
                Delete();
            else
                Save();

            return success;
        }


        public bool PersistSkills()
        {
            bool success = true;
            
            //persist all saved skill data
            foreach (SkillData d in SavedSkills.Values.ToList().Concat(UnsavedSkills))
            {
                bool skills = true;
                //delete if remove, else save
                if (d.Remove)
                    d.Delete();
                else
                    d.Save();

            }

            //load the new skill data

            Dictionary<int, SkillData> savedSkills;

            if (!DatabaseConnector.LoadColleagueSkills1(this, out savedSkills))
            {
                success = false;
            }

            SavedSkills = new ReadOnlyDictionary<int, SkillData>(savedSkills);
            Console.WriteLine("-------------Skills Saved " + UnsavedSkills.Count + ", Skills Loaded " + SavedSkills.Count);
            UnsavedSkills.Clear();

            return success;
        }


        public bool PersistEducationHistory()
        {
            bool success = true;

            //persist all saved education history data
            foreach (EducationData d in SavedEducationHistory.Values.Concat(UnsavedEducationHistory))
            {
                //delete if remove, else save
                if (d.Remove)
                    d.Delete();
                else
                    d.Save();


            }

            //load the new education history data

            Dictionary<int, EducationData> savedEducationHistory;

            if (!DatabaseConnector.LoadColleagueEducationHistory1(this, out savedEducationHistory))
            {
                success = false;
            }

            SavedEducationHistory = new ReadOnlyDictionary<int, EducationData>(savedEducationHistory);
            Console.WriteLine("-------------Education Saved " + UnsavedEducationHistory.Count + ", Education Loaded " + SavedEducationHistory.Count);
            UnsavedEducationHistory.Clear();

            return success;
        }


        public bool PersistCertifications()
        {
            bool success = true;

            //persist all saved education history data
            foreach (CertificationData d in SavedCertifications.Values.Concat(UnsavedCertifications))
            {
                //delete if remove, else save
                if (d.Remove)
                    d.Delete();
                else
                    d.Save();


            }

            //load the new education history data

            Dictionary<int, CertificationData> savedCertifications;

            if (!DatabaseConnector.LoadColleagueCertifications(this, out savedCertifications))
            {
                success = false;
            }

            SavedCertifications = new ReadOnlyDictionary<int, CertificationData>(savedCertifications);
            Console.WriteLine("-------------Certifications Saved " + UnsavedCertifications.Count + ", Certifications Loaded " + SavedCertifications.Count);
            UnsavedCertifications.Clear();

            return success;
        }


        public bool PersistWorkHistory()
        {
            bool success = true;
            
            //persist all saved work history data
            foreach (WorkData d in SavedWorkHistory.Values.Concat(UnsavedWorkHistory))
            {
                //delete if remove, else save
                if (d.Remove)
                    d.Delete();
                else
                    d.Save();


            }

            //load the new work history data

            Dictionary<int, WorkData> savedWorkHistory;

            if (!DatabaseConnector.LoadColleagueWorkHistory1(this, out savedWorkHistory))
            {
                success = false;
            }

            SavedWorkHistory = new ReadOnlyDictionary<int, WorkData>(savedWorkHistory);
            Console.WriteLine("-------------Work Saved " + UnsavedWorkHistory.Count + ", Work Loaded " + SavedWorkHistory.Count);
            UnsavedWorkHistory.Clear();

            return success;
        }


        public bool PersistProfiles()
        {
            bool success = true;

            //persist all saved profile data
            foreach (ProfileData d in SavedProfiles.Values.Concat(UnsavedProfiles))
            {
                //delete if remove, else save
                if (d.Remove)
                    d.Delete();
                else
                    d.Save();

            }

            //load the new profile data

            Dictionary<int, ProfileData> savedProfiles;

            if (!DatabaseConnector.LoadColleageProfiles(this, out savedProfiles))
                success = false;

            SavedProfiles = new ReadOnlyDictionary<int, ProfileData>(savedProfiles);
            Console.WriteLine("-------------Profiles Saved " + UnsavedProfiles.Count + ", Profiles Loaded " + SavedProfiles.Count);
            UnsavedProfiles.Clear();

            return success;
        }

        #endregion


    }
}

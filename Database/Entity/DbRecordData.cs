using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SCCPP1.Database.Tables;
using SCCPP1.User;
using SCCPP1.User.Data;

namespace SCCPP1.Database.Entity
{
    public abstract class DbRecordData
    {

        protected List<DbRecordData> InheritedRecordData { get; set; }

        public bool HasInheritedData { get { return InheritedRecordData.Count > 0; } }

        public RecordData Data;

        public DbTable Table;

        protected List<DataPair> _dataPairs;

        public int FieldCount { get { return _dataPairs.Count; } }


        private DataPair[] _columnsAndValues;
        public DataPair[] ColumnsAndValues
        {
            get 
            { 
                if (_columnsAndValues == null)
                   _columnsAndValues = _dataPairs.ToArray();
                return _columnsAndValues;
            }
            protected set { _columnsAndValues = value; }
        }




        public DbRecordData(RecordData data, string tableName)
        {
            Table = DatabaseConnector.TableModels.Tables[tableName];
            _dataPairs = new List<DataPair>();
            InheritedRecordData = new List<DbRecordData>();
            _nextCol = 0;
            Data = data;
        }


        protected void AddValue(string key, object value)
        {
            _dataPairs.Add(new DataPair(key, value));
        }


        protected void Add(string key, object value)
        {
            _dataPairs.Add(new DataPair(key, value));
        }



        private int _nextCol;

        protected void AddNextValue(object value)
        {
            Add(Table.Columns[_nextCol++].Name, value);
            if (_nextCol >= Table.Columns.Length)
                ColumnsAndValues = _dataPairs.ToArray();
        }


    }




    public class DbAccountRecord : DbRecordData
    {

        /**
         * 
         * 
                    cmd.Parameters.AddWithValue("@user_hash", Utilities.ToSHA256Hash(userID));

                    cmd.Parameters.AddWithValue("@role", role);
                    cmd.Parameters.AddWithValue("@name", ValueCleaner(name));
                    cmd.Parameters.AddWithValue("@email", ValueCleaner(email));
                    cmd.Parameters.AddWithValue("@phone", ValueCleaner(phone));
                    cmd.Parameters.AddWithValue("@address", ValueCleaner(address));
                    cmd.Parameters.AddWithValue("@intro_narrative", ValueCleaner(introNarrative));
                    cmd.Parameters.AddWithValue("@main_profile_id", ValueCleaner(mainProfileID));
         * 
         */
        public DbAccountRecord(Account account) : base(account, "colleagues")
        {
            AddNextValue(account.RecordID);
            AddNextValue(Utilities.ToSHA256Hash(account.GetUsername()));
            AddNextValue(account.Name);
            AddNextValue(account.EmailAddress);
            AddNextValue(account.PhoneNumber);
            AddNextValue(account.StreetAddress);
            AddNextValue(account.IntroNarrative);
            AddNextValue(account.MainProfileID);
        }


    }

    public class DbStateRecord : DbRecordData
    {

        public DbStateRecord(int recordID, string state, string abbreviation) : base(null, "states")
        {
            AddNextValue(recordID);
            AddNextValue(state);
            AddNextValue(abbreviation);
        }

    }

    public class DbSkillsRecord : DbRecordData
    {

        //Account owner, int recordID, string skillCategoryName, int skillCategoryID, string skillName, int skillID, int rating
        public DbSkillsRecord(int recordID, string skillName)
            : base(null, "skills")
        {
            AddNextValue(recordID);
            AddNextValue(skillName);
        }

    }

    public class DbSkillCategoriesRecord : DbRecordData
    {

        //Account owner, int recordID, string skillCategoryName, int skillCategoryID, string skillName, int skillID, int rating
        public DbSkillCategoriesRecord(int recordID, string skillCategory)
            : base(null, "skill_categories")
        {
            AddNextValue(recordID);
            AddNextValue(skillCategory);
        }

    }

    public class DbColleagueSkillsRecord : DbRecordData
    {

        public readonly DbSkillsRecord SkillsRecord;

        public readonly DbSkillCategoriesRecord SkillCategoriesRecord;


        //Account owner, int recordID, string skillCategoryName, int skillCategoryID, string skillName, int skillID, int rating
        public DbColleagueSkillsRecord(Account owner, int recordID, string skillCategoryName, int skillCategoryID, string skillName, int skillID, int rating)
            : base(null, "colleague_skills")
        {
            SkillsRecord = new DbSkillsRecord(skillID, skillName);
            SkillCategoriesRecord = new DbSkillCategoriesRecord(skillCategoryID, skillCategoryName);
            AddNextValue(recordID);
            AddNextValue(owner.RecordID);

            //get record id pointer?
            AddNextValue(SkillsRecord.ColumnsAndValues[0].Value);

            //get record id pointer?
            AddNextValue(SkillCategoriesRecord.ColumnsAndValues[0].Value);

            AddNextValue(rating);
        }


        //Account owner, int recordID, string skillCategoryName, int skillCategoryID, string skillName, int skillID, int rating
        public DbColleagueSkillsRecord(SkillData sd)
            : this(sd.Owner, sd.RecordID, sd.SkillCategoryName, sd.SkillCategoryID, sd.SkillName, sd.SkillID, sd.Rating)
        {
        }

    }
}

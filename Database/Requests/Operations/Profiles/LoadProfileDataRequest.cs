using Microsoft.Data.Sqlite;
using SCCPP1.User.Data;
using SCCPP1.User;
using System.Collections.Generic;

namespace SCCPP1.Database.Requests.Operations.Profiles
{
    public class LoadProfileDataRequest : OwnerRecordDataRequest
    {
        public LoadProfileDataRequest(Account account) : base(account) { }

        //populates an account's education data
        protected internal override bool RunCommand(SqliteCommand cmd)
        {

            Dictionary<int, ProfileData> dict = new();
            Result = dict;

            if (GetAccount() == null || GetAccount().RecordID < 0)
                return false;

            cmd.CommandText = @"SELECT id, colleague_id, title, about_section, colleague_skills_ids, education_history_ids, colleague_certs_ids, work_history_ids, ordering
                                FROM profiles
                                WHERE colleague_id=@colleague_id;";

            cmd.Parameters.AddWithValue("@colleague_id", GetAccount().RecordID);

            using (SqliteDataReader r = cmd.ExecuteReader())
            {
                ProfileData pd;
                string skillIDs, educationIDs, certIDs, workIDs;
                HashSet<int> skills, education, certs, work;

                void CreateHashSet(string values, out HashSet<int> hashSet)
                {
                    if (string.IsNullOrEmpty(values))
                        hashSet = new HashSet<int>();
                    else if (values.Length == 1)
                        (hashSet = new HashSet<int>()).Add(int.Parse(values));
                    else
                        hashSet = new HashSet<int>(values.Split(',').Select(int.Parse));
                }

                while (r.Read())
                {
                    skillIDs = GetString(r, 4);
                    educationIDs = GetString(r, 5);
                    certIDs = GetString(r, 6);
                    workIDs = GetString(r, 7);

                    CreateHashSet(skillIDs, out skills);
                    CreateHashSet(educationIDs, out education);
                    CreateHashSet(certIDs, out certs);
                    CreateHashSet(workIDs, out work);


                    pd = new ProfileData(
                        GetAccount(),
                        GetInt32(r, 0), //populate profile record id
                        GetString(r, 2), //populate title
                        skills, //populate skills ids
                        education, //populate education history ids
                        certs, //populate certification ids
                        work, //populate work history ids
                        GetString(r, 8) //populate ordering
                        );


                    string[] aboutVals = GetString(r, 3).Split('|');

                    pd.ShowName = aboutVals[0].Equals("true");
                    pd.ShowEmailAddress = aboutVals[1].Equals("true");
                    pd.ShowPhoneNumber = aboutVals[2].Equals("true");
                    pd.ShowIntroNarrative = aboutVals[3].Equals("true");

                    dict.TryAdd(pd.RecordID, pd);
                }
            }
            return true;
        }

    }
}

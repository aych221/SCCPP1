using Microsoft.Data.Sqlite;
using SCCPP1.User;

namespace SCCPP1.Database.Requests.Operations.Profiles
{
    public class SearchProfileKeyphraseRequest : OwnerRecordDataRequest
    {
        protected string _keyphrase;
        public SearchProfileKeyphraseRequest(Account account, string keyphrase)
            : base(account)
        {
            _keyphrase = keyphrase;
        }




        protected internal override bool RunCommand(SqliteCommand cmd)
        {
            //keyphrase is required, but return true since the code executed correctly

            if (_keyphrase == null)
                return true;

            ProfileResultSet resultSet = new ProfileResultSet();
            List<int> ids = new List<int>();

            //would eventually need a max limit to selection size
            //only select in batches
            //or exclude ids and continue search until max size is met, then batch size after
            cmd.CommandText = @"SELECT id, colleague_id, title, colleague_skills_ids, education_history_ids, colleague_certs_ids, work_history_ids
                            FROM profiles;";

            //first load all profiles
            using (SqliteDataReader r = cmd.ExecuteReader())
            {
                ProfileResult result;
                while (r.Read())
                {

                    result = new ProfileResult(
                        GetInt32(r, 0),
                        GetInt32(r, 1),
                        GetString(r, 2),
                        GetString(r, 3),
                        GetString(r, 4),
                        GetString(r, 5),
                        GetString(r, 6)
                        );


                    //check the title if a match was found,
                    //add to list and continue to next profile
                    if (result.Title != null && result.Title.Contains(_keyphrase))
                    {
                        ids.Add(result.RecordID);
                        continue;
                    }

                    resultSet.AddResult(result);
                }
            }


            //now find ids containing keyphrase

            List<int> colleagueIDs = SearchColleague(cmd),
                skillIDs = SearchSkills(cmd),
                educationIDs = SearchEducationHistory(cmd),
                certifcationIDs = SearchCertifications(cmd),
                workIDs = SearchWorkHistory(cmd);

            foreach (ProfileResult p in resultSet.Results)
            {
                if (CrossReferenceIDs(p.RecordID, ids, skillIDs, p.SkillRecordIDs)
                    || CrossReferenceIDs(p.RecordID, ids, educationIDs, p.EducationRecordIDs)
                    || CrossReferenceIDs(p.RecordID, ids, certifcationIDs, p.CertificationRecordIDs)
                    || CrossReferenceIDs(p.RecordID, ids, workIDs, p.WorkRecordIDs))
                    continue;
            }

            Result = ids;

            return true;
        }



        private bool CrossReferenceIDs(int profileID, List<int> outputIDList, List<int> listIDs, HashSet<int> profileIDSet)
        {
            foreach (int i in listIDs)
            {
                //match found, add profile to list and skip everything else
                if (profileIDSet.Contains(i))
                {
                    outputIDList.Add(profileID);
                    return true;
                }

            }

            //no match found
            return false;
        }

        private List<int> SearchColleague(SqliteCommand cmd)
        {
            return SearchForKeyphrase(cmd,
                @"SELECT id, name, email, phone, address, intro_narrative
                    FROM colleagues
                    WHERE (name LIKE @keyphrase OR email LIKE @keyphrase OR address LIKE @keyphrase OR intro_narrative LIKE @keyphrase);"
            );
        }



        private List<int> SearchSkills(SqliteCommand cmd)
        {
            return SearchForKeyphrase(cmd,
                @"SELECT cs.id, cs.colleague_id, sc.name AS skill_category, cs.skill_category_id, s.name AS skill, cs.skill_id, cs.rating
                    FROM colleague_skills cs
                    JOIN skills s ON cs.skill_id = s.id
                    LEFT JOIN skill_categories sc ON cs.skill_category_id = sc.id
                    WHERE (sc.name LIKE @keyphrase OR s.name LIKE @keyphrase);"
            );
        }



        private List<int> SearchEducationHistory(SqliteCommand cmd)
        {
            return SearchForKeyphrase(cmd,
                @"SELECT eh.id, eh.colleague_id, eh.education_type_id, eh.institution_id, eh.municipality_id, eh.state_id, eh.start_date, eh.end_date, eh.description, et.type AS education_type, i.name AS institution
                    FROM education_history eh
                    JOIN education_types et ON eh.education_type_id = et.id
                    JOIN institutions i ON eh.institution_id = i.id
                    WHERE (eh.description LIKE @keyphrase OR et.type LIKE @keyphrase OR i.name LIKE @keyphrase);"
            );
        }



        private List<int> SearchCertifications(SqliteCommand cmd)
        {
            return SearchForKeyphrase(cmd,
                @"SELECT cc.id, cc.colleague_id, cc.cert_type_id, cc.institution_id, cc.municipality_id, cc.state_id, cc.start_date, cc.end_date, cc.description, ct.type AS cert_type, i.name AS institution
                    FROM colleague_certs cc
                    JOIN cert_types ct ON cc.cert_type_id = ct.id
                    JOIN institutions i ON cc.institution_id = i.id
                    WHERE (cc.description LIKE @keyphrase OR ct.type LIKE @keyphrase OR i.name LIKE @keyphrase);"
            );
        }



        private List<int> SearchWorkHistory(SqliteCommand cmd)
        {
            return SearchForKeyphrase(cmd,
                @"SELECT wh.id, wh.colleague_id, wh.employer_id, wh.job_title_id, wh.municipality_id, wh.state_id, wh.start_date, wh.end_date, wh.description, e.name AS employer_name, jt.title AS job_title_name
                    FROM work_history wh
                    JOIN employers e ON wh.employer_id = e.id
                    JOIN job_titles jt ON wh.job_title_id = jt.id
                    WHERE (wh.description LIKE @keyphrase OR e.name LIKE @keyphrase OR jt.title LIKE @keyphrase);"
            );
        }


        /// <summary>
        /// Uses the parameter @keyphrase to search fields based on the input sql.
        /// This method will reset the command before it executes but will not reset it after.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="sql"></param>
        /// <returns>A list of ints containing the record ids that met the where condition.</returns>
        private List<int> SearchForKeyphrase(SqliteCommand cmd, string sql)
        {
            ResetCommand();

            List<int> ids = new List<int>();

            cmd.CommandText = sql;

            cmd.Parameters.AddWithValue("@keyphrase", ValueCleaner(_keyphrase));

            using (SqliteDataReader r = cmd.ExecuteReader())
                while (r.Read())
                    ids.Add(GetInt32(r, 0));

            return ids;
        }


    }

    internal class ProfileResultSet
    {
        public List<ProfileResult> Results;
        public Dictionary<int, HashSet<int>> SkillIDToProfileID;
        public Dictionary<int, HashSet<int>> EducationIDToProfileID;
        public Dictionary<int, HashSet<int>> CertificationIDToProfileID;
        public Dictionary<int, HashSet<int>> WorkIDToProfileID;

        public ProfileResultSet()
        {
            Results = new List<ProfileResult>();
            SkillIDToProfileID = new Dictionary<int, HashSet<int>>();
            EducationIDToProfileID = new Dictionary<int, HashSet<int>>();
            CertificationIDToProfileID = new Dictionary<int, HashSet<int>>();
            WorkIDToProfileID = new Dictionary<int, HashSet<int>>();
        }


        public void AddResult(ProfileResult result)
        {
            Results.Add(result);

            AddToDictionary(result.RecordID, result.SkillRecordIDs, SkillIDToProfileID);
            AddToDictionary(result.RecordID, result.EducationRecordIDs, EducationIDToProfileID);
            AddToDictionary(result.RecordID, result.CertificationRecordIDs, CertificationIDToProfileID);
            AddToDictionary(result.RecordID, result.WorkRecordIDs, WorkIDToProfileID);
        }


        private void AddToDictionary(int recordID, HashSet<int> recordIDs, Dictionary<int, HashSet<int>> dict)
        {
            HashSet<int> profileIDs;
            foreach (int i in recordIDs)
            {
                if (dict.TryGetValue(i, out profileIDs))
                {
                    profileIDs.Add(recordID);
                }
                else
                {
                    profileIDs = new HashSet<int>() { recordID };
                    dict.Add(i, profileIDs);
                }
            }
        }
    }



    internal class ProfileResult
    {
        public int RecordID, ColleagueID;

        public HashSet<int> SkillRecordIDs, EducationRecordIDs, CertificationRecordIDs, WorkRecordIDs;

        public string Title;

        public string FullName;

        public string IntroNarative;

        public string EmailAddress;

        public string PhoneNumber;


        public ProfileResult(int profileID, int colleagueID, string title, string skillIDsCsv, string eduIDsCsv, string certIDsCsv, string workIDsCsv)
        {
            RecordID = profileID;
            Title = title;
            ColleagueID = colleagueID;
            SkillRecordIDs = CreateHashSet(skillIDsCsv);
            EducationRecordIDs = CreateHashSet(eduIDsCsv);
            CertificationRecordIDs = CreateHashSet(certIDsCsv);
            WorkRecordIDs = CreateHashSet(workIDsCsv);
        }


        HashSet<int> CreateHashSet(string values)
        {
            if (string.IsNullOrEmpty(values))
                return new HashSet<int>();
            else if (values.Length == 1)
                return new HashSet<int>() { int.Parse(values) };
            else
                return new HashSet<int>(values.Split(',').Select(int.Parse));
        }

    }
}

using Microsoft.Data.Sqlite;
using SCCPP1.User;
using SCCPP1.User.Data;

namespace SCCPP1.Database.Requests.Operations.Education
{
    public class LoadEducationDataRequest : OwnerRecordDataRequest
    {
        public LoadEducationDataRequest(Account account) : base(account) { }

        //populates an account's education data
        protected internal override bool RunCommand(SqliteCommand cmd)
        {

            Dictionary<int, EducationData> dict = new();
            Result = dict;

            if (GetAccount() == null || GetAccount().RecordID < 0)
                return false;

            cmd.CommandText = @"SELECT eh.id, eh.colleague_id, eh.education_type_id, eh.institution_id, eh.municipality_id, eh.state_id, eh.start_date, eh.end_date, eh.description, et.type AS education_type, i.name AS institution
                                FROM education_history eh
                                JOIN education_types et ON eh.education_type_id = et.id
                                JOIN institutions i ON eh.institution_id = i.id
                                WHERE eh.colleague_id=@colleague_id;";

            cmd.Parameters.AddWithValue("@colleague_id", GetAccount().RecordID);

            using (SqliteDataReader r = cmd.ExecuteReader())
            {
                EducationData ed;
                while (r.Read())
                {
                    ed = new EducationData(GetAccount(), GetInt32(r, 0));
                    ed.EducationTypeID = GetInt32(r, 2);
                    ed.InstitutionID = GetInt32(r, 3);
                    ed.Location = new Location(GetInt32(r, 4), GetInt32(r, 5));
                    ed.StartDate = GetDateOnly(r, 6);
                    ed.EndDate = GetDateOnly(r, 7);
                    ed.Description = GetString(r, 8);
                    ed.EducationType = GetString(r, 9);
                    ed.Institution = GetString(r, 10);

                    dict.TryAdd(ed.RecordID, ed);
                }
            }
            return true;
        }

    }
}

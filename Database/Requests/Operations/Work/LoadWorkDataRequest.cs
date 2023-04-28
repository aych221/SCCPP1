using Microsoft.Data.Sqlite;
using SCCPP1.User.Data;
using SCCPP1.User;
using System.Collections.Generic;

namespace SCCPP1.Database.Requests.Operations.Work
{
    public class LoadWorkDataRequest : OwnerRecordDataRequest
    {
        public LoadWorkDataRequest(Account account) : base(account) { }

        //populates an account's education data
        protected internal override bool RunCommand(SqliteCommand cmd)
        {

            Dictionary<int, WorkData> dict = new();
            Result = dict;

            if (GetAccount() == null || GetAccount().RecordID < 0)
                return false;

            cmd.CommandText = @"SELECT wh.id, wh.colleague_id, wh.employer_id, wh.job_title_id, wh.municipality_id, wh.state_id, wh.start_date, wh.end_date, wh.description, e.name AS employer_name, jt.title AS job_title_name
                                FROM work_history wh
                                JOIN employers e ON wh.employer_id = e.id
                                JOIN job_titles jt ON wh.job_title_id = jt.id
                                WHERE wh.colleague_id=@colleague_id;";

            cmd.Parameters.AddWithValue("@colleague_id", GetAccount().RecordID);

            using (SqliteDataReader r = cmd.ExecuteReader())
            {
                WorkData wd;
                while (r.Read())
                {
                    wd = new WorkData(GetAccount(), GetInt32(r, 0));
                    wd.EmployerID = GetInt32(r, 2);
                    wd.JobTitleID = GetInt32(r, 3);
                    wd.Location = new Location(GetInt32(r, 4), GetInt32(r, 5));
                    wd.StartDate = GetDateOnly(r, 6);
                    wd.EndDate = GetDateOnly(r, 7);
                    wd.Description = GetString(r, 8);
                    wd.Employer = GetString(r, 9);
                    wd.JobTitle = GetString(r, 10);

                    dict.TryAdd(wd.RecordID, wd);
                }
            }
            return true;
        }

    }
}

using Microsoft.Data.Sqlite;
using SCCPP1.User;
using SCCPP1.User.Data;

namespace SCCPP1.Database.Requests.Operations.Skills
{
    public class LoadSkillDataRequest : OwnerRecordDataRequest
    {
        public LoadSkillDataRequest(Account account) : base(account) { }

        //populates an account's skill data
        protected internal override bool RunCommand(SqliteCommand cmd)
        {

            Dictionary<int, SkillData> dict = new();
            Result = dict;

            if (GetAccount() == null || GetAccount().RecordID < 0)
                return false;

            cmd.CommandText = @"SELECT cs.id, cs.colleague_id, sc.name AS skill_category, cs.skill_category_id, s.name AS skill, cs.skill_id, cs.rating
                                FROM colleague_skills cs
                                JOIN skills s ON cs.skill_id = s.id
                                LEFT JOIN skill_categories sc ON cs.skill_category_id = sc.id
                                WHERE cs.colleague_id=@colleague_id;";

            cmd.Parameters.AddWithValue("@colleague_id", GetAccount().RecordID);

            using (SqliteDataReader r = cmd.ExecuteReader())
            {
                SkillData sd;
                while (r.Read())
                {
                    //SkillData(Account owner, int recordID, string skillCategoryName, int skillCategoryID, string skillName, int skillID, int rating)
                    sd = new SkillData(GetAccount(), GetInt32(r, 0), GetString(r, 2), GetInt32(r, 3), GetString(r, 4), GetInt32(r, 5), GetInt32(r, 6));

                    dict.TryAdd(sd.RecordID, sd);
                }
            }
            return true;
        }

    }
}

using Microsoft.Data.Sqlite;
using SCCPP1.User;
using SCCPP1.User.Data;
using System.Data;

namespace SCCPP1.Database.Requests.Operations.Skills
{
    public class PersistSkillDataRequest : OwnerRecordDataRequest
    {
        protected SkillData _data;

        public PersistSkillDataRequest(SkillData data)
            : base(data.Owner)
        {
            _data = data;
        }

        //persists a single skill record
        protected internal override bool RunCommand(SqliteCommand cmd)
        {
            if (_data.Remove)
            {
                _data.IsRemoved = Delete(cmd, "colleague_skills", _data.RecordID);
                Console.WriteLine("Removed Skill " + _data.SkillName + "? " + _data.IsRemoved);
                return _data.IsRemoved;
            }

            //skill name is required, but return true since the code executed correctly
            if (_data.SkillName == null)
                return true;

            _data.SkillID = PersistSingleValue(cmd, "skills", "name", _data.SkillName);

            //check if null (since category is not required to be set)
            if (_data.SkillCategoryName != null)
                _data.SkillCategoryID = PersistSingleValue(cmd, "skill_categories", "name", _data.SkillCategoryName);

            if (_data.RecordID > 0)
            {
                cmd.CommandText = @"UPDATE colleague_skills
                                    SET skill_id=@skill_id, skill_category_id=@skill_category_id, rating=@rating
                                    WHERE id=@id;";

                cmd.Parameters.AddWithValue("@id", _data.RecordID);
                cmd.Parameters.AddWithValue("@skill_id", _data.SkillID);
                cmd.Parameters.AddWithValue("@skill_category_id", ValueCleaner(_data.SkillCategoryID));
                cmd.Parameters.AddWithValue("@rating", ValueCleaner(_data.Rating));

                cmd.ExecuteNonQuery();
            }
            else
            {
                cmd.CommandText = @"INSERT INTO colleague_skills(colleague_id, skill_id, skill_category_id, rating)
                                    VALUES (@colleagueID, @skill_id, @skill_category_id, @rating)
                                    RETURNING id;";

                cmd.Parameters.AddWithValue("@colleagueID", _data.Owner.RecordID);
                cmd.Parameters.AddWithValue("@skill_id", _data.SkillID);
                cmd.Parameters.AddWithValue("@skill_category_id", ValueCleaner(_data.SkillCategoryID));
                cmd.Parameters.AddWithValue("@rating", ValueCleaner(_data.Rating));


                object? id = cmd.ExecuteScalar();

                if (id == null)
                    return false;

                _data.RecordID = Convert.ToInt32(id);
            }


            return true;
        }


    }
}

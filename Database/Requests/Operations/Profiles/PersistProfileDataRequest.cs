using Microsoft.Data.Sqlite;
using SCCPP1.User.Data;

namespace SCCPP1.Database.Requests.Operations.Profiles
{
    public class PersistProfileDataRequest : OwnerRecordDataRequest
    {
        protected ProfileData _data;

        public PersistProfileDataRequest(ProfileData data)
            : base(data.Owner)
        {
            _data = data;
        }

        //persists a single skill record
        protected internal override bool RunCommand(SqliteCommand cmd)
        {
            if (_data.Remove)
                return _data.IsRemoved = Delete(cmd, "profiles", _data.RecordID);

            //these fields are required, but return true since the code executed correctly
            if (_data.Title == null)
                return true;

            void AddCommonParameters()
            {
                cmd.Parameters.AddWithValue("@colleague_id", ValueCleaner(_data.Owner.RecordID));
                cmd.Parameters.AddWithValue("@title", ValueCleaner(_data.Title));
                cmd.Parameters.AddWithValue("@about_section", ValueCleaner($"{_data.ShowName}|{_data.ShowEmailAddress}|{_data.ShowPhoneNumber}|{_data.ShowIntroNarrative}"));
                cmd.Parameters.AddWithValue("@colleague_skills_ids", ValueCleaner(string.Join(",", _data.SelectedSkillIDs)));
                cmd.Parameters.AddWithValue("@education_history_ids", ValueCleaner(string.Join(",", _data.SelectedEducationIDs)));
                cmd.Parameters.AddWithValue("@colleague_certs_ids", ValueCleaner(string.Join(",", _data.SelectedCertificationIDs)));
                cmd.Parameters.AddWithValue("@work_history_ids", ValueCleaner(string.Join(",", _data.SelectedWorkIDs)));
                cmd.Parameters.AddWithValue("@ordering", ValueCleaner(_data.Ordering));
            }

            if (_data.RecordID > 0)
            {
                cmd.CommandText = @"UPDATE profiles
                                    SET colleague_id=@colleague_id, title=@title, about_section=@about_section, colleague_skills_ids=@colleague_skills_ids, education_history_ids=@education_history_ids, colleague_certs_ids=@colleague_certs_ids, work_history_ids=@work_history_ids, ordering=@ordering 
                                    WHERE id=@id;";

                cmd.Parameters.AddWithValue("@id", _data.RecordID);
                AddCommonParameters();

                cmd.ExecuteNonQuery();
            }
            else
            {
                cmd.CommandText = @"INSERT INTO profiles (colleague_id, title, about_section, colleague_skills_ids, education_history_ids, colleague_certs_ids, work_history_ids, ordering) 
                                    VALUES (@colleague_id, @title, @about_section, @colleague_skills_ids, @education_history_ids, @colleague_certs_ids, @work_history_ids, @ordering)
                                    RETURNING id;";

                AddCommonParameters();

                object? id = cmd.ExecuteScalar();

                if (id == null)
                    return false;

                _data.RecordID = Convert.ToInt32(id);
            }


            return true;
        }


    }
}

using Microsoft.Data.Sqlite;
using SCCPP1.User.Data;

namespace SCCPP1.Database.Requests.Operations
{
    public class CertificationDataRequest : OwnerRecordDataRequest
    {
        protected CertificationData _data;

        public CertificationDataRequest(CertificationData data)
            : base(data.Owner)
        {
            _data = data;
        }


        /*
            CREATE TABLE cert_types (
              id INTEGER PRIMARY KEY AUTOINCREMENT,
              type TEXT UNIQUE NOT NULL
            );
        
            CREATE TABLE colleague_certs (
              id INTEGER PRIMARY KEY AUTOINCREMENT,
              colleague_id INTEGER NOT NULL,
              cert_type_id INTEGER NOT NULL,
              institution_id INTEGER NOT NULL,
              municipality_id INTEGER,
              state_id INTEGER,
              start_date DATE,
              end_date DATE,
              description TEXT,
              FOREIGN KEY (colleague_id) REFERENCES colleagues(id) ON DELETE CASCADE,
              FOREIGN KEY (cert_type_id) REFERENCES cert_types(id),
              FOREIGN KEY (institution_id) REFERENCES institutions(id),
              FOREIGN KEY (municipality_id) REFERENCES municipalities(id),
              FOREIGN KEY (state_id) REFERENCES states(id)
            );
         */

        protected internal override bool RunCommand(SqliteCommand cmd)
        {
            if (_data.Remove)
            {
                _data.IsRemoved = Delete(cmd, "colleague_certs", _data.RecordID);
                Console.WriteLine("Removed Certification " + _data.CertificationType + "? " + _data.IsRemoved);
                return _data.IsRemoved;
            }

/*            //type is required, but return true since the code executed correctly
            if (_data.CertificationType == null)
                return true;

            _data.CertificationTypeID = PersistSingleValue(cmd, "cert_types", "type", _data.CertificationType);

            //check if null (since category is not required to be set)
            if (_data.Institution != null)
                _data.InstitutionID = PersistSingleValue(cmd, "institutions", "name", _data.Institution);

            if (_data.RecordID > 0)
            {
                cmd.CommandText = @"UPDATE colleague_certs
                                    SET cert=@skill_id, skill_category_id=@skill_category_id, rating=@rating
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
            }*/


            return true;
        }


    }
}

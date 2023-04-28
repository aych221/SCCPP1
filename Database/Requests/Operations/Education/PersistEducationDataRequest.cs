using Microsoft.Data.Sqlite;
using SCCPP1.Models;
using SCCPP1.User;
using SCCPP1.User.Data;
using System.Data;

namespace SCCPP1.Database.Requests.Operations.Education
{
    public class PersistEducationDataRequest : OwnerRecordDataRequest
    {
        protected EducationData _data;

        public PersistEducationDataRequest(EducationData data)
            : base(data.Owner)
        {
            _data = data;
        }

        //persists a single profile record
        protected internal override bool RunCommand(SqliteCommand cmd)
        {
            if (_data.Remove)
                return _data.IsRemoved = Delete(cmd, "education_history", _data.RecordID);

            //these fields are required, but return true since the code executed correctly
            if (_data.EducationType == null || _data.Institution == null)
                return true;

            _data.EducationTypeID = PersistSingleValue(cmd, "education_types", "type", _data.EducationType);
            _data.InstitutionID = PersistSingleValue(cmd, "institutions", "name", _data.Institution);

            void AddCommonParameters()
            {
                cmd.Parameters.AddWithValue("@colleague_id", _data.Owner.RecordID);
                cmd.Parameters.AddWithValue("@education_type_id", _data.EducationTypeID);
                cmd.Parameters.AddWithValue("@institution_id", _data.InstitutionID);
                cmd.Parameters.AddWithValue("@municipality_id", ValueCleaner(_data.Location.MunicipalityID));
                cmd.Parameters.AddWithValue("@state_id", ValueCleaner(_data.Location.StateID));
                cmd.Parameters.AddWithValue("@start_date", ValueCleaner(_data.StartDate));
                cmd.Parameters.AddWithValue("@end_date", ValueCleaner(_data.EndDate));
                cmd.Parameters.AddWithValue("@description", ValueCleaner(_data.Description));
            }

            if (_data.RecordID > 0)
            {
                cmd.CommandText = @"UPDATE education_history
                                    SET colleague_id=@colleague_id, education_type_id=@education_type_id, institution_id=@institution_id, municipality_id=municipality_id, state_id=@state_id, start_date=@start_date, end_date=@end_date, description=@description
                                    WHERE id=@id;";

                cmd.Parameters.AddWithValue("@id", _data.RecordID);
                AddCommonParameters();

                cmd.ExecuteNonQuery();
            }
            else
            {
                cmd.CommandText = @"INSERT INTO education_history(colleague_id, education_type_id, institution_id, municipality_id, state_id, start_date, end_date, description) 
                                    VALUES (@colleague_id, @education_type_id, @institution_id, @municipality_id, @state_id, @start_date, @end_date, @description)
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

using Microsoft.Data.Sqlite;
using SCCPP1.Models;
using SCCPP1.User.Data;

namespace SCCPP1.Database.Requests.Operations.Work
{
    public class PersistWorkDataRequest : OwnerRecordDataRequest
    {
        protected WorkData _data;

        public PersistWorkDataRequest(WorkData data)
            : base(data.Owner)
        {
            _data = data;
        }

        //persists a single skill record
        protected internal override bool RunCommand(SqliteCommand cmd)
        {
            if (_data.Remove)
                return _data.IsRemoved = Delete(cmd, "work_history", _data.RecordID);

            //these fields are required, but return true since the code executed correctly
            if (_data.Employer == null || _data.JobTitle == null)
                return true;

            _data.EmployerID = PersistSingleValue(cmd, "employers", "name", _data.Employer);
            _data.JobTitleID = PersistSingleValue(cmd, "job_titles", "title", _data.JobTitle);

            void AddCommonParameters()
            {
                cmd.Parameters.AddWithValue("@colleague_id", _data.Owner.RecordID);
                cmd.Parameters.AddWithValue("@employer_id", _data.EmployerID);
                cmd.Parameters.AddWithValue("@job_title_id", _data.JobTitleID);
                cmd.Parameters.AddWithValue("@municipality_id", ValueCleaner(_data.Location.MunicipalityID));
                cmd.Parameters.AddWithValue("@state_id", ValueCleaner(_data.Location.StateID));
                cmd.Parameters.AddWithValue("@start_date", ValueCleaner(_data.StartDate));
                cmd.Parameters.AddWithValue("@end_date", ValueCleaner(_data.EndDate));
                cmd.Parameters.AddWithValue("@description", ValueCleaner(_data.Description));
            }

            if (_data.RecordID > 0)
            {
                cmd.CommandText = @"UPDATE work_history
                                    SET colleague_id=@colleague_id, employer_id=@employer_id, job_title_id=@job_title_id, municipality_id=municipality_id, state_id=@state_id, start_date=@start_date, end_date=@end_date, description=@description
                                    WHERE id=@id;";


                cmd.Parameters.AddWithValue("@id", _data.RecordID);
                AddCommonParameters();

                cmd.ExecuteNonQuery();
            }
            else
            {
                cmd.CommandText = @"INSERT INTO work_history(colleague_id, employer_id, job_title_id, municipality_id, state_id, start_date, end_date, description) 
                                    VALUES (@colleague_id, @employer_id, @job_title_id, @municipality_id, @state_id, @start_date, @end_date, @description)
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

using Microsoft.Data.Sqlite;
using SCCPP1.User;
using SCCPP1.User.Data;
using System.Data;
using System.Net;

namespace SCCPP1.Database.Requests.Operations.Colleague
{
    public class PersistColleagueDataRequest : RecordDataRequest
    {
        protected Account _data;
        public PersistColleagueDataRequest(Account data)
            : base(data)
        {
            _data = data;
        }

        //persists an account's primary data
        protected internal override bool RunCommand(SqliteCommand cmd)
        {
            if (_data.Remove)
                return _data.IsRemoved = Delete(cmd, "colleagues", _data.RecordID);

            //these fields are required, but return true since the code executed correctly
            if (_data.GetUsername() == null || _data.Name == null)
                return true;

            void AddCommonParameters()
            {
                cmd.Parameters.AddWithValue("@user_hash", Utilities.ToSHA256Hash(_data.GetUsername()));
                cmd.Parameters.AddWithValue("@role", _data.Role);
                cmd.Parameters.AddWithValue("@name", ValueCleaner(_data.Name));
                cmd.Parameters.AddWithValue("@email", ValueCleaner(_data.EmailAddress));
                cmd.Parameters.AddWithValue("@phone", ValueCleaner(_data.PhoneNumber));
                cmd.Parameters.AddWithValue("@address", ValueCleaner(_data.StreetAddress));
                cmd.Parameters.AddWithValue("@intro_narrative", ValueCleaner(_data.IntroNarrative));
                cmd.Parameters.AddWithValue("@main_profile_id", ValueCleaner(_data.MainProfileID));
            }

            if (_data.RecordID > 0)
            {
                cmd.CommandText = @"UPDATE colleagues
                                    SET user_hash=@user_hash, role=@role, name=@name, email=@email, phone=@phone, address=@address, intro_narrative=@intro_narrative, main_profile_id=@main_profile_id
                                    WHERE id = @id;";


                cmd.Parameters.AddWithValue("@id", _data.RecordID);
                AddCommonParameters();

                cmd.ExecuteNonQuery();
            }
            else
            {
                cmd.CommandText = @"INSERT INTO colleagues (user_hash, role, name, email, phone, address, intro_narrative, main_profile_id)
                                    VALUES (@user_hash, @role, @name, @email, @phone, @address, @intro_narrative, @main_profile_id)
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

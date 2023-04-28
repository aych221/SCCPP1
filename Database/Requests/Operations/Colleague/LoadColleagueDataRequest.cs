using Microsoft.Data.Sqlite;
using SCCPP1.User.Data;
using SCCPP1.User;
using System.Data;
using SCCPP1.Session;

namespace SCCPP1.Database.Requests.Operations.Colleague
{
    public class LoadColleagueDataRequest : RecordDataRequest
    {

        public LoadColleagueDataRequest(SessionData sessionData) : base(sessionData) { }


        //populates an account's primary data
        protected internal override bool RunCommand(SqliteCommand cmd)
        {
            if (GetSessionData() == null || GetSessionData().Username == null)
            {
                #if DEBUG_HANDLER
                    //still give a new account for debugging purposes
                    Result = new Account(GetSessionData().Username);
                #endif

                return false;
            }

            cmd.CommandText = @"SELECT id, role, name, email, phone, address, intro_narrative, main_profile_id
                                FROM colleagues
                                WHERE (user_hash=@user_hash);";

            cmd.Parameters.AddWithValue("@user_hash", GetSessionData().Username);

            using (SqliteDataReader r = cmd.ExecuteReader(CommandBehavior.SingleRow))
            {

                //account was found.
                if (r.Read())
                {
                    //load new instance with basic colleague information
                    Account account = new Account(GetSessionData(), true);

                    account.RecordID = GetInt32(r, 0);
                    account.Role = GetInt32(r, 1);
                    account.Name = GetString(r, 2);
                    account.EmailAddress = GetString(r, 3);
                    account.PhoneNumber = GetInt64(r, 4);
                    account.StreetAddress = GetString(r, 5);
                    account.IntroNarrative = GetString(r, 6);
                    account.MainProfileID = GetInt32(r, 7);

                    Result = account;
                    return true;
                }

            }

            Result = new Account(GetSessionData(), false);
            return true;
        }

    }
}

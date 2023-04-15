using Microsoft.EntityFrameworkCore.Diagnostics;
using SCCPP1.Database.Tables;
using SCCPP1.User;

namespace SCCPP1.Database.Entity
{
    public abstract class DbRecordData
    {

        public RecordData Data;

        public DbColumn[] Columns;

        public DbRecordData(RecordData data)
        {
            Data = data;
            _nextCol = 0;
        }

        private int _nextCol;

        protected void AddNextValue(object value)
        {
            Columns[_nextCol++].Value = value;
        }

    }


    public class DbColleagueData : DbRecordData
    {

        /**
         * 
         * 
                    cmd.Parameters.AddWithValue("@user_hash", Utilities.ToSHA256Hash(userID));

                    cmd.Parameters.AddWithValue("@role", role);
                    cmd.Parameters.AddWithValue("@name", ValueCleaner(name));
                    cmd.Parameters.AddWithValue("@email", ValueCleaner(email));
                    cmd.Parameters.AddWithValue("@phone", ValueCleaner(phone));
                    cmd.Parameters.AddWithValue("@address", ValueCleaner(address));
                    cmd.Parameters.AddWithValue("@intro_narrative", ValueCleaner(introNarrative));
                    cmd.Parameters.AddWithValue("@main_profile_id", ValueCleaner(mainProfileID));
         * 
         */
        public DbColleagueData(Account account) : base(account)
        {
            DbTable table = DatabaseConnector.TableModels.Tables["colleagues"];

            Columns = table.CopyColumnsNoPK();

            AddNextValue(Utilities.ToSHA256Hash(account.GetUsername()));
            AddNextValue(account.Name);
            AddNextValue(account.EmailAddress);
            AddNextValue(account.PhoneNumber);
            AddNextValue(account.StreetAddress);
            AddNextValue(account.IntroNarrative);
            AddNextValue(account.MainProfileID);
        }


    }

    public class DbStateData : DbRecordData
    {

        public DbStateData(string state, string abbreviation) : base(null)
        {
            DbTable table = DatabaseConnector.TableModels.Tables["states"];

            Columns = table.CopyColumnsNoPK();

            AddNextValue(state);
            AddNextValue(abbreviation);
        }

    }
}

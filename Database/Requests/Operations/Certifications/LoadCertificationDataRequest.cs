using Microsoft.Data.Sqlite;
using SCCPP1.User.Data;
using SCCPP1.User;

namespace SCCPP1.Database.Requests.Operations.Certifications
{
    public class LoadCertificationDataRequest : OwnerRecordDataRequest
    {

        public LoadCertificationDataRequest(Account account) : base(account) { }


        //populates an account's education data
        protected internal override bool RunCommand(SqliteCommand cmd)
        {

            Dictionary<int, CertificationData> dict = new();
            Result = dict;

            if (GetAccount() == null || GetAccount().RecordID < 0)
                return false;

            cmd.CommandText = @"SELECT cc.id, cc.colleague_id, cc.cert_type_id, cc.institution_id, cc.municipality_id, cc.state_id, cc.start_date, cc.end_date, cc.description, ct.type AS cert_type, i.name AS institution
                                FROM colleague_certs cc
                                JOIN cert_types ct ON cc.cert_type_id = ct.id
                                JOIN institutions i ON cc.institution_id = i.id
                                WHERE cc.colleague_id=@colleague_id;";

            cmd.Parameters.AddWithValue("@colleague_id", GetAccount().RecordID);

            using (SqliteDataReader r = cmd.ExecuteReader())
            {
                CertificationData cd;
                while (r.Read())
                {
                    //Account owner, int recordID, string institution, int institutionID, string certificateType, int certificateTypeID, string? description, Location? location, DateOnly? startDate, DateOnly? endDate
                    cd = new CertificationData(
                        GetAccount(),
                        GetInt32(r, 0),
                        GetString(r, 10),
                        GetInt32(r, 3),
                        GetString(r, 9),
                        GetInt32(r, 2),
                        GetString(r, 8),
                        new Location(GetInt32(r, 4), GetInt32(r, 5)),
                        GetDateOnly(r, 6),
                        GetDateOnly(r, 7)
                        );

                    dict.TryAdd(cd.RecordID, cd);
                }
            }
            return true;
        }

    }
}

namespace SCCPP1.Database.Entity
{
    public class DbTuple
    {

        public DbColumn[] Columns { get; set; }


        public DbTuple(params DbColumn[] columns)
        {
            Columns = columns;
        }

    }

    public class DbRecord : DbTuple
    {

        public DbRecordData RecordData;

        public DbRecord(DbRecordData recordData) :
            base(recordData.Table.Columns)
        {
            RecordData = recordData;
        }
    }


}

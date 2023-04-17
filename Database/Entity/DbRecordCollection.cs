using SCCPP1.User;
using SCCPP1.User.Data;

namespace SCCPP1.Database.Entity
{
    public abstract class DbRecordCollection
    {

        public DbTable Table { get; protected set; }

        protected List<DbRecordData> Records { get; set; }


        public DbRecordCollection()
        {
            Records = new List<DbRecordData>();
        }

        public DbRecordCollection(DbRecordData record) : this()
        {
            Add(record);
        }

        public DbRecordCollection(DbRecordData[] records) : this()
        {
            AddRange(records);
        }


        public DbRecordCollection(DbRecordCollection records) : this()
        {
            AddRange(records);
        }





        public void Add(DbRecordData record)
        {
            RecordCheck(record);

            Records.Add(record);
        }


        public void AddRange(DbRecordData[] records)
        {
            RecordCheck(records[0]);

            Records.AddRange(records);
        }



        public void AddRange(DbRecordCollection records)
        {
            RecordCheck(records[0]);
            Records.AddRange(records.Records);
        }


        private bool RecordCheck(DbRecordData record)
        {
            if (Table == null)
                Table = record.Table;
            else if (!Table.Equals(record.Table))
                throw new InvalidColumnCollection("Record does not belong to this table.", record);
            return true;
        }


        public void Remove(DbRecordData record)
        {
            Records.Remove(record);
        }


        public void RemoveAt(int index)
        {
            Records.RemoveAt(index);
        }


        public void Clear()
        {
            Records.Clear();
        }


        public bool Contains(DbRecordData record)
        {
            return Records.Contains(record);
        }



        public int IndexOf(DbRecordData record)
        {
            return Records.IndexOf(record);
        }


        public void Insert(int index, DbRecordData record)
        {
            Records.Insert(index, record);
        }



        public void InsertRange(int index, DbRecordData[] records)
        {
            Records.InsertRange(index, records);
        }



        public void InsertRange(int index, DbRecordCollection records)
        {
            Records.InsertRange(index, records.Records);
        }





        public DbRecordData[] ToArray()
        {
            return Records.ToArray();
        }


        public DbRecordData this[int index]
        {
            get { return Records[index]; }
            set { Records[index] = value; }
        }


        public int Count
        {
            get { return Records.Count; }
        }


    }

    //may not use
    public class InvalidColumnCollection : Exception
    {
        public object InvalidValue { get; }

        public InvalidColumnCollection(string message, object invalidValue) : base(message)
        {
            InvalidValue = invalidValue;
        }
    }

}

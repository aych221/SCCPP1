using System.Drawing;

namespace SCCPP1.Database.Entity
{
    public class DataPair
    {
        public string Key { get; set; }
        public object Value { get; set; }


        public DataPair(string key, object value)
        {
            Key = key;
            Value = value;
        }


        public DataPair(DataPair data)
        {
            Key = data.Key;
            Value = data.Value;
        }



        public static DataPair CopyKey(DataPair data)
        {
            return new DataPair(data.Key, default);
        }



        public override string ToString()
        {
            return $"{Key} = {Value}";
        }

    }
}

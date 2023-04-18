using SCCPP1.Database.Entity;

namespace SCCPP1.User
{
    public abstract class RecordData
    {

        public int RecordID { get; set; }


        /// <summary>
        /// Returns true if the information in the account is up-to-date with changes made in the database.
        /// </summary>
        protected bool IsUpdated;

        /// <summary>
        /// A flag to let the database know the account information in here needs a save.
        /// </summary>
        protected bool NeedsSave;

        /// <summary>
        /// A flag to let the database know this record needs to be removed if it exists, or not to save if it doesn't exist.
        /// </summary>
        public bool Remove;


        private bool _isRemoved = false;
        /// <summary>
        /// This is true if the record has been removed from the database. False if it has not been removed. (This is not a remove flag, see <see cref="Remove"/>)
        /// </summary>
        public bool IsRemoved { 
            get { return _isRemoved; }
            set { _isRemoved = value; }
        }


        /// <summary>
        /// Message that is used to relay information back after a save attempt was made
        /// </summary>
        protected string SaveAttemptMessage { get; set; }


        public DbColumn DbReference { get; protected set; }


        public RecordData(int id = -1)
        {
            this.RecordID = id;

            this.IsUpdated = false;
            this.NeedsSave = false;
        }

        /// <summary>
        /// Used to set a field to it's local value while setting a save flag for the database if it is changed.
        /// </summary>
        /// <typeparam name="T">variable type being used</typeparam>
        /// <param name="field">field to be referenced</param>
        /// <param name="value">value to set the field to</param>
        protected void SetField<T>(ref T field, T value)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                NeedsSave = true;
                field = value;
            }
        }

        public abstract bool Save();


        public abstract bool Delete();




    }
}

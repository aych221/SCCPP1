namespace SCCPP1.User.Data
{
    public struct Location : IEquatable<Location>
    {
        public string State { get; set; }
        public int StateID { get; set; }

        public string StateAbbreviation { get; set; }

        public string Municipality { get; set; }
        public int MunicipalityID { get; set; }

        public Location(int municipalityID, int stateID)
        {
            this.StateID = stateID;
            this.MunicipalityID = municipalityID;

            string s = DatabaseConnector.GetCachedState(stateID);

            this.State = s.Substring(0, 2);
            this.StateAbbreviation = s.Substring(2);

            this.Municipality = DatabaseConnector.GetCachedMunicipality(municipalityID);
        }


        public bool Equals(Location other)
        {
            return State.Equals(other.State) && Municipality.Equals(other.Municipality);
        }

    }
}

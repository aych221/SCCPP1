namespace SCCPP1.Database.Policies
{
    public enum ForeignKeyDeletePolicy
    {
        None,
        SetNull,
        SetDefault,
        Cascade,
        Restrict,
        NoAction
    }
}

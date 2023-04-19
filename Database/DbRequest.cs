using SCCPP1.User;

namespace SCCPP1.Database
{
    public class DbRequest
    {
        private Account _account;

        public DbRequest(Account account)
        {
            _account = account;
        }

        public Account GetAccount()
        {
            return _account;
        }
    }
}

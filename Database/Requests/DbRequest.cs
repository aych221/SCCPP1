using Microsoft.Data.Sqlite;
using SCCPP1.User;

namespace SCCPP1.Database.Requests
{

    public abstract class DbRequest
    {
        public enum RequestStatus
        {
            PENDING,
            PROCESSING,
            COMPLETED
        }

        public RequestStatus Status { get; set; }

        private Account _account;
        private DbRequestHandler _handler;
        private bool _isExecuting;

        protected internal DbRequest(Account account)
        {
            _account = account;
            Status = RequestStatus.PENDING;
        }

        public Account GetAccount() => _account;

        internal void SetHandler(DbRequestHandler handler)
        {
            // Only set once
            if (_handler == null)
            {
                _handler = handler;
            }
        }

        //used as a wrapper for RunCommand so that we can set the handler;
        //this prevents this request from being processed by another handler
        internal bool Execute(DbRequestHandler handler)
        {
            if (_handler != null)
                throw new System.Exception("This request is already being processed by another handler.");

            SetHandler(handler);
            return RunCommand(handler.Command);
        }


        public abstract bool RunCommand(SqliteCommand cmd);
    }


    public class InsertDbRequest : DbRequest
    {
        private string _tableName;
        private Dictionary<string, object> _values;

        public InsertDbRequest(Account account, string tableName, Dictionary<string, object> values) : base(account)
        {
            _tableName = tableName;
            _values = values;
        }

        public override bool RunCommand(SqliteCommand cmd)
        {
            cmd.CommandText = $"INSERT INTO {_tableName} ({string.Join(",", _values.Keys)}) VALUES ({string.Join(",", _values.Values.Select(v => $"@{v}"))})";
            foreach (var vals in _values)
            {
                cmd.Parameters.AddWithValue($"@{vals.Key}", vals.Value);
            }

            return true;
        }
    }

    public class UpdateDbRequest : DbRequest
    {
        private string _tableName;
        private Dictionary<string, object> _values;
        private string _whereClause;

        public UpdateDbRequest(Account account, string tableName, Dictionary<string, object> values, string whereClause) : base(account)
        {
            _tableName = tableName;
            _values = values;
            _whereClause = whereClause;
        }

        public override bool RunCommand(SqliteCommand cmd)
        {
            var setValues = string.Join(",", _values.Select(kvp => $"{kvp.Key} = @{kvp.Key}"));
            cmd.CommandText = $"UPDATE {_tableName} SET {setValues} WHERE {_whereClause}";
            foreach (var vals in _values)
            {
                cmd.Parameters.AddWithValue($"@{vals.Key}", vals.Value);
            }

            return true;
        }
    }

    public class SelectDbRequest<T> : DbRequest
    {
        private string _tableName;
        private string _whereClause;
        private Func<SqliteDataReader, T> _resultSelector;

        public SelectDbRequest(Account account, string tableName, string whereClause, Func<SqliteDataReader, T> resultSelector) : base(account)
        {
            _tableName = tableName;
            _whereClause = whereClause;
            _resultSelector = resultSelector;
        }

        public override bool RunCommand(SqliteCommand cmd)
        {
            cmd.CommandText = $"SELECT * FROM {_tableName} WHERE {_whereClause}";

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var result = _resultSelector(reader);
                }
            }

            return true;
        }
    }

}

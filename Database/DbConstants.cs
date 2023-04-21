namespace SCCPP1.Database
{

    static class DbConstants
    {

        /// <summary>
        /// Primarily used to determine default priorities of requests.
        /// </summary>
        private enum PriorityLevel
        {
            IMMEDIATE = int.MinValue, //only used for initial database commands.
            HIGHEST = 0,
            HIGH = 100,
            NORMAL = 200,
            LOW = 300,
            LOWEST = 400
        }

        /// <summary>
        /// Used to assign default priority levels to different query commands.
        /// </summary>
        enum CommandPriorities
        {
            INSERT = PriorityLevel.LOW,
            INSERT_OR_IGNORE = PriorityLevel.LOW,
            UPDATE = PriorityLevel.LOW,
            SELECT = PriorityLevel.LOW,
            DELETE = PriorityLevel.NORMAL,
            ALTER = PriorityLevel.HIGHEST,
            CREATE = PriorityLevel.IMMEDIATE,
            DROP = PriorityLevel.IMMEDIATE
        }

        public sealed class QueryStringPlaceholders
        {

            private const string HEADER = "${";
            private const string FOOTER = ";";

            private const string TABLE_NAME = @"table_name";
            private const string COLUMNS = @"column_names";
            private const string VALUES = @"values";

            private const string COLUMNS_EQUALS_VALUES = @"columns_equals_values";

            private const string RETURNING_CLAUSE = @"returning_clause";

            private const string WHERE_CLAUSE = @"where_clause";
            private const string DELETE_CLAUSE = @"delete_clause";
            private const string JOIN_CLAUSE = @"join_clause";
            private const string LEFT_JOIN_CLAUSE = @"left_join_clause";

            private const string PRIMARY_KEY_CLAUSE = @"primary_key_clause";
            private const string FOREIGN_KEY_CLAUSE = @"foreign_key_clause";

            //will be used for any other insertable strings for query generation
            public static string ToPlaceholder(string parameter) => HEADER + parameter + FOOTER;


            //these will be cached at runtime startup,
            //so it's okay to use the method to generate once.

            public static string TableName => ToPlaceholder(TABLE_NAME);

            public static string ColumnNames => ToPlaceholder(COLUMNS);

            public static string Values => ToPlaceholder(VALUES);

            public static string ColumnsEqualsValues => ToPlaceholder(COLUMNS_EQUALS_VALUES);

            public static string ReturningClause => ToPlaceholder(RETURNING_CLAUSE);

            public static string DeleteClause => ToPlaceholder(DELETE_CLAUSE);

            public static string WhereClause => ToPlaceholder(WHERE_CLAUSE);

            public static string JoinClause => ToPlaceholder(JOIN_CLAUSE);

            public static string LeftJoinClause => ToPlaceholder(LEFT_JOIN_CLAUSE);

            public static string PrimaryKeyClause => ToPlaceholder(PRIMARY_KEY_CLAUSE);

            public static string ForeignKeyClause => ToPlaceholder(FOREIGN_KEY_CLAUSE);


        }

        //clean up every 5 minutes.
        public const int HANDLER_CLEANUP_INTERVAL = 5 * 60 * 1000;
        public const int HANDLER_SLEEP_TIME = 100;
        public const int SLEEP_TIME = 100;
        public const int CONNECTION_POOL_SIZE = 10;
        public const int MAX_ACCOUNTS_PER_HANDLER = (100 / CONNECTION_POOL_SIZE);


    }
}
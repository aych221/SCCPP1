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
        public const int HANDLER_CLEANUP_INTERVAL = 30 * 1000;// 5 * 60 * 1000;
        public const int HANDLER_SLEEP_TIME = 100;
        public const int SLEEP_TIME = 100;
        public const int CONNECTION_POOL_SIZE = 10;
        public const int MAX_ACCOUNTS_PER_HANDLER = (100 / CONNECTION_POOL_SIZE);


        public const bool STARTUP_RESET_TABLES = false;
        public const bool STARTUP_SCHEMA_CHECK = false;
        public const bool STARTUP_ADD_MOCK_DATA = false;


        public static string StartupSql()
        {
            string sql = "";
            if (STARTUP_RESET_TABLES)
                sql += _resetTablesSql;
            if (STARTUP_SCHEMA_CHECK)
                sql += _schemaCheckSql;
            return sql;
        }


        private const string _resetTablesSql = @"

        PRAGMA foreign_keys = 0;
        BEGIN TRANSACTION; 
        DROP TABLE IF EXISTS [colleagues];
        DROP TABLE IF EXISTS [municipalities];
        DROP TABLE IF EXISTS [states];
        DROP TABLE IF EXISTS [education_types];
        DROP TABLE IF EXISTS [institutions];
        DROP TABLE IF EXISTS [education_history];
        DROP TABLE IF EXISTS [employers];
        DROP TABLE IF EXISTS [job_titles];
        DROP TABLE IF EXISTS [work_history];
        DROP TABLE IF EXISTS [skills];
        DROP TABLE IF EXISTS [skill_categories];
        DROP TABLE IF EXISTS [colleague_skills];
        DROP TABLE IF EXISTS [cert_types];
        DROP TABLE IF EXISTS [colleague_certs];
        DROP TABLE IF EXISTS [profiles];
        COMMIT;
        PRAGMA foreign_keys = 1;

        ";
        ///<summary>
        ///This is initial query used for creating the initial database and tables.
        ///Should only be used if the database is being completely reset or initially created.
        ///</summary>
        private const string _schemaCheckSql = @"

        CREATE TABLE IF NOT EXISTS colleagues (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          user_hash TEXT UNIQUE NOT NULL,
          role INTEGER NOT NULL, --0=admin 1=normal
          name TEXT NOT NULL,
          email TEXT,
          phone INTEGER,
          address TEXT,
          intro_narrative TEXT
        );

        CREATE TABLE IF NOT EXISTS municipalities (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          name TEXT UNIQUE NOT NULL
        );

        CREATE TABLE IF NOT EXISTS states (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          name TEXT NOT NULL,
          abbreviation CHAR(2)
        );

        CREATE TABLE IF NOT EXISTS institutions (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          name TEXT UNIQUE NOT NULL
        );

        CREATE TABLE IF NOT EXISTS education_types (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          type TEXT UNIQUE NOT NULL
        );

        CREATE TABLE IF NOT EXISTS education_history (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          colleague_id INTEGER NOT NULL,
          education_type_id INTEGER NOT NULL,
          institution_id INTEGER NOT NULL,
          municipality_id INTEGER,
          state_id INTEGER,
          start_date DATE,
          end_date DATE,
          description TEXT,
          FOREIGN KEY (colleague_id) REFERENCES colleagues(id) ON DELETE CASCADE,
          FOREIGN KEY (education_type_id) REFERENCES education_types(id),
          FOREIGN KEY (institution_id) REFERENCES institutions(id),
          FOREIGN KEY (municipality_id) REFERENCES municipalities(id),
          FOREIGN KEY (state_id) REFERENCES states(id)
        );
        
        CREATE TABLE IF NOT EXISTS cert_types (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          type TEXT UNIQUE NOT NULL
        );
        
        CREATE TABLE IF NOT EXISTS colleague_certs (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          colleague_id INTEGER NOT NULL,
          cert_type_id INTEGER NOT NULL,
          institution_id INTEGER NOT NULL,
          municipality_id INTEGER,
          state_id INTEGER,
          start_date DATE,
          end_date DATE,
          description TEXT,
          FOREIGN KEY (colleague_id) REFERENCES colleagues(id) ON DELETE CASCADE,
          FOREIGN KEY (cert_type_id) REFERENCES cert_types(id),
          FOREIGN KEY (institution_id) REFERENCES institutions(id),
          FOREIGN KEY (municipality_id) REFERENCES municipalities(id),
          FOREIGN KEY (state_id) REFERENCES states(id)
        );


        CREATE TABLE IF NOT EXISTS employers (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          name TEXT UNIQUE NOT NULL
        );

        CREATE TABLE IF NOT EXISTS job_titles (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          title TEXT UNIQUE NOT NULL
        );

        CREATE TABLE IF NOT EXISTS work_history (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          colleague_id INTEGER NOT NULL,
          employer_id INTEGER NOT NULL,
          job_title_id INTEGER NOT NULL,
          municipality_id INTEGER,
          state_id INTEGER,
          start_date DATE,
          end_date DATE,
          description TEXT,
          FOREIGN KEY (colleague_id) REFERENCES colleagues(id) ON DELETE CASCADE,
          FOREIGN KEY (employer_id) REFERENCES employers(id),
          FOREIGN KEY (municipality_id) REFERENCES municipalities(id),
          FOREIGN KEY (state_id) REFERENCES states(id),
          FOREIGN KEY (job_title_id) REFERENCES job_titles(id)
          );

        CREATE TABLE IF NOT EXISTS skills (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          name TEXT UNIQUE NOT NULL
        );

        CREATE TABLE IF NOT EXISTS skill_categories (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT UNIQUE NOT NULL
        );

        CREATE TABLE IF NOT EXISTS colleague_skills (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          colleague_id INTEGER NOT NULL,
          skill_id INTEGER NOT NULL,
          skill_category_id INTEGER,
          rating INTEGER,
          FOREIGN KEY (colleague_id) REFERENCES colleagues(id) ON DELETE CASCADE,
          FOREIGN KEY (skill_id) REFERENCES skills(id),
          FOREIGN KEY (skill_category_id) REFERENCES skill_categories(id)
        );

        CREATE TABLE IF NOT EXISTS profiles (
          id INTEGER PRIMARY KEY AUTOINCREMENT,
          colleague_id INTEGER NOT NULL,
          title TEXT NOT NULL,          --similar to the file name
          about_section TEXT,           --about section of the profile
          colleague_skills_ids TEXT,    --list of colleague_skills ids in specified order
          education_history_ids TEXT,   --list of education history ids in specified order
          colleague_certs_ids TEXT,     --list of colleague_certs ids in specified order
          work_history_ids TEXT,        --list of work history ids in specified order
          ordering TEXT,                --The ordering of how the different sections will be (education, work, skills, etc)
          FOREIGN KEY (colleague_id) REFERENCES colleagues(id) ON DELETE CASCADE
        );

        ALTER TABLE colleagues ADD COLUMN IF NOT EXISTS main_profile_id INTEGER REFERENCES profiles(id) ON DELETE CASCADE;
        
        --Populate states table, since this likely will not change.
        INSERT INTO states (name, abbreviation)
        VALUES
          ('Alabama', 'AL'),
          ('Alaska', 'AK'),
          ('Arizona', 'AZ'),
          ('Arkansas', 'AR'),
          ('California', 'CA'),
          ('Colorado', 'CO'),
          ('Connecticut', 'CT'),
          ('Delaware', 'DE'),
          ('Florida', 'FL'),
          ('Georgia', 'GA'),
          ('Hawaii', 'HI'),
          ('Idaho', 'ID'),
          ('Illinois', 'IL'),
          ('Indiana', 'IN'),
          ('Iowa', 'IA'),
          ('Kansas', 'KS'),
          ('Kentucky', 'KY'),
          ('Louisiana', 'LA'),
          ('Maine', 'ME'),
          ('Maryland', 'MD'),
          ('Massachusetts', 'MA'),
          ('Michigan', 'MI'),
          ('Minnesota', 'MN'),
          ('Mississippi', 'MS'),
          ('Missouri', 'MO'),
          ('Montana', 'MT'),
          ('Nebraska', 'NE'),
          ('Nevada', 'NV'),
          ('New Hampshire', 'NH'),
          ('New Jersey', 'NJ'),
          ('New Mexico', 'NM'),
          ('New York', 'NY'),
          ('North Carolina', 'NC'),
          ('North Dakota', 'ND'),
          ('Ohio', 'OH'),
          ('Oklahoma', 'OK'),
          ('Oregon', 'OR'),
          ('Pennsylvania', 'PA'),
          ('Rhode Island', 'RI'),
          ('South Carolina', 'SC'),
          ('South Dakota', 'SD'),
          ('Tennessee', 'TN'),
          ('Texas', 'TX'),
          ('Utah', 'UT'),
          ('Vermont', 'VT'),
          ('Virginia', 'VA'),
          ('Washington', 'WA'),
          ('West Virginia', 'WV'),
          ('Wisconsin', 'WI'),
          ('Wyoming', 'WY');
        ";

    }
}
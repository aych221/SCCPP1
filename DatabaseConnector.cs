using Microsoft.Data.Sqlite;
using SCCPP1.Entity;

namespace SCCPP1
{
    public class DatabaseConnector
    {
        public Account user { get; private set; }

        private static string connStr = @"Data Source=CPPDatabse.db";

        public void Load()
        {

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                using (SqliteCommand cmd = new SqliteCommand(dbSQL, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            //printusers();
        }




        private static void printusers()
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT *  FROM account;";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            Console.WriteLine($"'{r.GetString(1)}', '{r.GetString(2)}'");//user
                        }

                    }
                }
            }
        }

        public static Account LoadUser(string username, string password)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();
                string sql = @"SELECT *  FROM account WHERE (user_hash=@user AND pass_hash=@pass);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@user", Utilities.ToSHA256Hash(username));
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            //account = new Account(r.GetInt32(0), username, r.GetString(3));
                            return null;
                        }

                        Console.WriteLine("No Account");
                        return null;
                    }
                }
            }
        }


        public static void SaveLogout(Account account)
        {
            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();//acc, show, seats
                string sql = @"INSERT INTO [session] (acc_id, end_time) VALUES (@id, @end);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    //cmd.Parameters.AddWithValue("@id", account.getID());
                    cmd.Parameters.AddWithValue("@end", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        //TODO
        public static void SaveLogout()
        {
            /*if (account == null)
                return;*/

            using (SqliteConnection conn = new SqliteConnection(connStr))
            {
                conn.Open();//acc, show, seats
                string sql = @"INSERT INTO [session] (acc_id, end_time) VALUES (@id, @end);";
                using (SqliteCommand cmd = new SqliteCommand(sql, conn))
                {
                    //cmd.Parameters.AddWithValue("@id", account.getID());
                    //cmd.Parameters.AddWithValue("@end", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
            }
        }


        /*
		 * 
		USE master;
		ALTER DATABASE MovieSchedulingDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

		DROP DATABASE IF EXISTS MovieSchedulingDB;


		CREATE DATABASE MovieSchedulingDB;

		USE MovieSchedulingDB;*/

        private const string dbSQL = @" 


					BEGIN TRANSACTION; 
                    DROP TABLE IF EXISTS [account];
                    DROP TABLE IF EXISTS [session];
                    COMMIT;

CREATE TABLE [account] (
ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
  
user_hash CHAR(64) UNIQUE,
pass_hash CHAR(64), --Hash storage in SQL datatype
acc_role VARCHAR(100)
);

CREATE TABLE [session] (
ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
  
acc_id INTEGER,
end_time time, 
  
FOREIGN KEY (acc_id) REFERENCES account(ID)
);

CREATE TABLE [movie] (
ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
  
title VARCHAR(100) UNIQUE,
time_in_sec INTEGER
);

CREATE TABLE [theatre] (
ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
  
capacity INTEGER
);

CREATE TABLE [showing] (
ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
  
theatre_id INTEGER,
movie_id INTEGER, 
start_time time, -- Needs to be worked on
end_time time, --Needs to be worked on
  
FOREIGN KEY (theatre_id) REFERENCES [theatre](ID),
FOREIGN KEY (movie_id) REFERENCES [movie](ID)
);

CREATE TABLE [ticket] (
ID INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
  
acc_id INTEGER,
showing_id INTEGER, 
type_movie VARCHAR(100),
amount INTEGER,
  
FOREIGN KEY (acc_id) REFERENCES [account](ID),
FOREIGN KEY (showing_id) REFERENCES [showing](ID)
);

insert into account (user_hash, pass_hash, acc_role)
values('d80426c1f7bc58a13ea79e4776acf8cb35d1c1b6f8daf5cf22f2a964ec38632a', '0f49dcfc100202a827a6176dcbe67439d38d1668628345eb34adc5cc171bfd49', 'employee'), --('admin@move.theatre', 'Passwerd1!', 'admin')
('944c15b7bcdc8282b4c9d32410098ab46ad9b6829190ce52a07dca69b826b22c', 'b0f8ce197d3a9bd8234b98901ba0722abe20522ddc2a2471ae60b3c67577f0d4', 'customer'); --('number1cust@aol.net', 'someSafePass11', 'customer')


insert into theatre(capacity)
values(150),
(175),
(135),
(145);

insert into movie( title, time_in_sec)
values('Inglorious Basterds', 11580), --2 hours and 33 minutes 1
('Iron Man 3', 7800), --2 hours and 10 minutes 2
('The Batman', 10560), --2 hours and 56 minutes 3
('Top Gun: Maverick', 7860), --2 Hours and 11 minutes 4
('Forest Gump', 8520), --2 hours and 22 minutes 5
('Fight Club', 8340), --2 hours and 19 minutes 6
('The Truman Show', 6420), --1 hour and 47 minutes 7
('Interstellar', 10140), --2 hours and 49 minutes 8
('Pulp Fiction', 9240), --2 hours and 34 minutes 9
('The Godfather', 10500); --2 hours and 55 minutes 10

insert into showing(theatre_id, movie_id, start_time, end_time)
values(1, 9, '12:00:00', '14:34:00' ),
(1, 1, '12:30:00', '15:03:00' ),
(1, 2, '13:00:00', '15:10:00' ),
(1, 5, '13:30:00', '15:52:00' ),
(1, 7, '14:00:00', '15:47:00' ),
(1, 10, '14:30:00', '17:25:00' ),
(2, 8, '12:00:00' , '14:49:00' ),
(2, 5, '12:30:00' , '14:52:00' ),
(2, 3, '13:00:00' , '15:56:00' ),
(2, 1, '13:30:00' , '16:03:00' ),
(2, 6, '14:00:00' , '16:19:00' ),
(3, 2, '12:00:00' , '14:10:00' ),
(3, 5, '12:30:00' , '14:52:00'),
(3, 3, '13:00:00' , '15:56:00'),
(3, 7, '13:30:00' , '15:17:00' ),
(3, 4, '14:00:00' , '15:11:00' ),
(4, 10, '12:00:00' , '14:55:00' ),
(4, 5, '12:30:00' , '14:52:00' ),
(4, 1, '13:00:00' , '15:33:00' ),
(4, 8, '13:30:00' , '16:19:00' ),
(4, 6, '14:00:00' , '16:19:00' );
		";

    }
}

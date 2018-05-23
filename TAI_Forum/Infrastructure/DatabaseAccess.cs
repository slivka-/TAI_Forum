using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Text;
using System.Security.Cryptography;
using System.Linq;

namespace TAI_Forum.Infrastructure
{
    public class DatabaseAccess
    {
        private string connectionString;

        private static volatile DatabaseAccess instance;
        private static object syncRoot = new object();

        public static DatabaseAccess Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new DatabaseAccess();
                    }
                }
                return instance;
            }
        }

        private DatabaseAccess()
        {
            ConnectionStringSettings c = ConfigurationManager.ConnectionStrings["SqliteConnectionString"];
            connectionString = c.ConnectionString.Replace("{AppDir}", AppDomain.CurrentDomain.BaseDirectory);
        }

        #region Account

        public bool IsLoginFree(string login)
        {
            int counter = 0;
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = string.Format("SELECT * FROM Users WHERE Name = '{0}'", login);
                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            counter++;
                        }
                    }
                }
                conn.Close();
            }
            return counter == 0;
        }

        public bool RegisterUser(string login, string password)
        {
            bool output = false;
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                int newId = GetNextId("Users");

                if (newId > 0)
                {
                    byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                    byte[] hash = SHA256.Create().ComputeHash(passwordBytes);

                    string insertSql = string.Format("INSERT INTO Users(ID, Name, Password, Type) VALUES('{0}', '{1}', '{2}', 1)", newId, login, hash.ToHexString());
                    using (SQLiteCommand cmd = new SQLiteCommand(insertSql, conn))
                    {
                        if (cmd.ExecuteNonQuery() == 1)
                            output = true;
                    }
                }
                conn.Close();
            }
            return output;
        }

        public Tuple<bool, bool> LoginUser(string login, string password)
        {
            Tuple<bool, bool> output = Tuple.Create(false, false);
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = SHA256.Create().ComputeHash(passwordBytes);
                string countSql = string.Format("SELECT * FROM Users WHERE Name = '{0}' AND Password = '{1}'", login, hash.ToHexString());
                List<int> sqlOutput = new List<int>();
                using (SQLiteCommand cmd = new SQLiteCommand(countSql, conn))
                {
                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            sqlOutput.Add(int.Parse(rdr[3].ToString()));
                        }
                    }
                }
                conn.Close();
                if (sqlOutput.Count == 1)
                {
                    bool isAdmin = sqlOutput.First() == 42;
                    output = Tuple.Create(true, isAdmin);
                }
            }
            return output;
        }

        public Tuple<string, string> GetUserInfo(string login)
        {
            Tuple<string, string> output = null;
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = string.Format("SELECT Name,Type FROM Users WHERE Name = '{0}'", login);
                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            output = Tuple.Create(rdr[0].ToString(), rdr[1].ToString());
                        }
                    }
                }
                conn.Close();
            }
            return output;
        }

        public bool ConfirmUserPassword(string login, string password)
        {
            bool output = false;
            string hash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(password)).ToHexString();
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = string.Format("SELECT Password FROM Users WHERE Name = '{0}'", login);
                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            output = rdr[0].ToString().Equals(hash);
                        }
                    }
                }
                conn.Close();
            }
            return output;
        }

        public bool ChangeUserPassword(string login, string newPassword)
        {
            bool output = false;
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (var t = conn.BeginTransaction())
                {
                    string newHash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(newPassword)).ToHexString();

                    string sql = string.Format("UPDATE Users SET Password = '{0}' WHERE Name = '{1}'", newHash, login);
                    using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                    {
                        int r = cmd.ExecuteNonQuery();
                        if (r != 1)
                        {
                            t.Rollback();
                            return false;
                        }
                        output = true;
                        t.Commit();
                    }
                }
                conn.Close();
            }
            return output;
        }

        public Tuple<bool, string> ChangeUserLogin(string oldLogin, string newLogin)
        {
            Tuple<bool, string> output = null;
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (var t = conn.BeginTransaction())
                {
                    string sql = string.Format("UPDATE Users SET Name = '{0}' WHERE Name = '{1}'", newLogin, oldLogin);
                    using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                    {
                        int r = cmd.ExecuteNonQuery();
                        if (r != 1)
                        {
                            t.Rollback();
                            return Tuple.Create(false, "");
                        }
                        output = Tuple.Create(true, newLogin);
                        t.Commit();
                    }
                }
                conn.Close();
            }
            return output;
        }

        #endregion

        #region Threads

        public Tuple<bool,int> RegisterNewThread(string login, string topic, string content, string tags)
        {
            int threadId = GetNextId("Threads");
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string threadInsertSql = string.Format("INSERT INTO Threads(ID, Name, Tags) VALUES({0},'{1}','{2}')", threadId, topic, tags);
                        using (SQLiteCommand tCmd = new SQLiteCommand(threadInsertSql, conn))
                        {
                            int x = tCmd.ExecuteNonQuery();
                            if (x != 1)
                            {
                                transaction.Rollback();
                                return Tuple.Create(false,0);
                            }
                        }
                        int ordNum = GetNewMessageOrdNum(threadId);
                        int userId = GetUserId(login);
                        string currentTime = DateTime.Now.ToDbString();
                        string messageInsertSql = string.Format("INSERT INTO Messages(ThreadID, OrdNum, Content, PostDate, Score, UserID) VALUES({0}, {1}, '{2}', '{3}', {4}, {5})", threadId, ordNum, content, currentTime, 0, userId);
                        using (SQLiteCommand mCmd = new SQLiteCommand(messageInsertSql, conn))
                        {
                            int x = mCmd.ExecuteNonQuery();
                            if (x != 1)
                            {
                                transaction.Rollback();
                                return Tuple.Create(false, 0);
                            }
                        }
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return Tuple.Create(false, 0);
                    }
                }
                conn.Close();
            }
            return Tuple.Create(true, threadId); ;
        }

        public List<Tuple<int, string, string, string, string, string>> GetAllThreads(string tag = null)
        {
            List<Tuple<int, string, string, string, string, string>> output = new List<Tuple<int, string, string, string, string, string>>();
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string countSql = "";
                if (tag == null)
                    countSql = string.Format("select T.ID, T.Name, substr(M.Content,0,30), T.Tags, M.PostDate, U.Name from Threads T LEFT JOIN Messages M ON T.ID = M.ThreadID LEFT JOIN Users U ON M.UserID = U.ID WHERE M.OrdNum = 1;");
                else
                    countSql = string.Format("select T.ID, T.Name, substr(M.Content,0,30), T.Tags, M.PostDate, U.Name from Threads T LEFT JOIN Messages M ON T.ID = M.ThreadID LEFT JOIN Users U ON M.UserID = U.ID WHERE M.OrdNum = 1 AND T.Tags LIKE '%{0}%';", tag);

                using (SQLiteCommand cmd = new SQLiteCommand(countSql, conn))
                {
                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            output.Add(Tuple.Create(int.Parse(rdr[0].ToString()), rdr[1].ToString(), rdr[2].ToString(), rdr[3].ToString(), rdr[4].ToString(), rdr[5].ToString()));
                        }
                    }
                }
                conn.Close();
            }
            return output;
        }

        public bool DeleteThread(int threadId)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    string msgDelSql = string.Format("DELETE FROM Messages WHERE ThreadId = {0}", threadId);
                    using (SQLiteCommand cmd = new SQLiteCommand(msgDelSql, conn))
                    {
                        int x = cmd.ExecuteNonQuery();
                        if (x <= 0)
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }

                    string thrDelSql = string.Format("DELETE FROM Threads WHERE ID = {0}", threadId);
                    using (SQLiteCommand cmd = new SQLiteCommand(thrDelSql, conn))
                    {
                        int x = cmd.ExecuteNonQuery();
                        if (x != 1)
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }
                    transaction.Commit();
                }
                conn.Close();
            }
            return true;
        }

        public Tuple<string,string> GetThreadTitleAndTags(int threadId)
        {
            Tuple<string, string> output = null;
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string countSql = string.Format("SELECT Name, Tags FROM Threads WHERE ID = {0}", threadId);
                using (SQLiteCommand cmd = new SQLiteCommand(countSql, conn))
                {
                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            output = Tuple.Create(rdr[0].ToString(), rdr[1].ToString());
                        }
                    }
                }
                conn.Close();
            }

            return output;
        }

        public List<Tuple<string, string, int, string, int>> GetThreadContent(int threadId)
        {
            List<Tuple<string, string, int, string, int>> output = new List<Tuple<string, string, int, string, int>>();
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string countSql = string.Format("SELECT M.Content, M.PostDate, M.Score, U.Name, M.OrdNum FROM Messages M LEFT JOIN Users U ON M.UserID = U.ID WHERE M.ThreadID = {0} ORDER BY M.OrdNum ASC", threadId);
                using (SQLiteCommand cmd = new SQLiteCommand(countSql, conn))
                {
                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            output.Add(Tuple.Create(rdr[0].ToString(), rdr[1].ToString(), int.Parse(rdr[2].ToString()), rdr[3].ToString(), int.Parse(rdr[4].ToString())));
                        }
                    }
                }
                conn.Close();
            }
            return output;
        }

        public List<string> GetAllTags()
        {
            List<string> output = new List<string>();
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string countSql = string.Format("SELECT Tags FROM Threads");
                using (SQLiteCommand cmd = new SQLiteCommand(countSql, conn))
                {
                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            output.AddRange(rdr[0].ToString().Replace("#","").Replace(";", " ").Replace(",", " ").Replace(".", " ").Trim().Split(' ').Where(w => !w.Equals(" ")));
                        }
                    }
                }
                conn.Close();
            }
            if (output.Count > 0)
                output.GroupBy(g => g).Select(s => new
                {
                    Count = s.Count(),
                    Name = s.Key,
                    Tag = s.First()
                }).OrderByDescending(o => o.Count).Select(s => s.Tag).ToList();
            return output;
        }

        #endregion

        #region Messages

        public Tuple<bool, int> AddNewMessage(int threadId, string content, string userLogin)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int userId = GetUserId(userLogin);
                        int ordNum = GetNewMessageOrdNum(threadId);
                        string currentTime = DateTime.Now.ToDbString();
                        string messageInsertSql = string.Format("INSERT INTO Messages(ThreadID, OrdNum, Content, PostDate, Score, UserID) VALUES({0}, {1}, '{2}', '{3}', {4}, {5})", threadId, ordNum, content, currentTime, 0, userId);
                        using (SQLiteCommand mCmd = new SQLiteCommand(messageInsertSql, conn))
                        {
                            int x = mCmd.ExecuteNonQuery();
                            if (x != 1)
                            {
                                transaction.Rollback();
                                return Tuple.Create(false, 0);
                            }
                        }
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return Tuple.Create(false, 0);
                    }
                }
                conn.Close();
            }
            return Tuple.Create(true, threadId); ;
        }

        public int ScoreMessage(int threadId, int ordNum, char action)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string countSql = string.Format("UPDATE MESSAGES SET Score = Score{0}1 WHERE ThreadID = {1} AND OrdNum = {2};", action, threadId, ordNum);
                using (SQLiteCommand cmd = new SQLiteCommand(countSql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
            return threadId;
        }

        public Tuple<bool, int> DeleteMessage(int threadId, int ordNum)
        {
            Tuple<bool, int> output = Tuple.Create(false, 0);
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                using (var t = conn.BeginTransaction())
                {
                    string countSql = string.Format("DELETE FROM Messages WHERE ThreadID = {0} AND OrdNum = {1};", threadId, ordNum);
                    using (SQLiteCommand cmd = new SQLiteCommand(countSql, conn))
                    {
                        if (cmd.ExecuteNonQuery() != 1)
                        {
                            t.Rollback();
                            return output;
                        }
                        t.Commit();
                        output =  Tuple.Create(true, threadId);
                    }
                }
                conn.Close();
            }
            return output;
        }

        #endregion  

        #region tools

        private int GetNextId(string tableName)
        {
            int newId = 0;
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string countSql = string.Format("SELECT IFNULL(MAX(ID),0) FROM {0}", tableName);
                using (SQLiteCommand cmd = new SQLiteCommand(countSql, conn))
                {
                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            newId = int.Parse(rdr[0].ToString()) + 1;
                        }
                    }
                }
                conn.Close();
            }

            return newId;
        }

        private int GetNewMessageOrdNum(int threadId)
        {
            int newOrdNum = 0;
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string countSql = string.Format("SELECT IFNULL(MAX(OrdNum),0) FROM Messages WHERE ThreadID = {0}", threadId);
                using (SQLiteCommand cmd = new SQLiteCommand(countSql, conn))
                {
                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            newOrdNum = int.Parse(rdr[0].ToString()) + 1;
                        }
                    }
                }
                conn.Close();
            }
            return newOrdNum;
        }

        private int GetUserId(string name)
        {
            int userId = 0;
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string countSql = string.Format("SELECT ID FROM Users WHERE Name = '{0}'", name);
                using (SQLiteCommand cmd = new SQLiteCommand(countSql, conn))
                {
                    using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            userId = int.Parse(rdr[0].ToString());
                        }
                    }
                }
                conn.Close();
            }
            return userId;
        }

        #endregion
    }
}
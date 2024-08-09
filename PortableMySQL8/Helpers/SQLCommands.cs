#region License

/*

Copyright 2023 mewtwo0641
(See ADDITIONAL_COPYRIGHTS.txt for full list of copyright holders)

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS “AS IS” AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

#endregion License

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySql.Data.MySqlClient;

namespace PortableMySQL8
{
    public class SQLCommands
    {
        public enum MySqlAdminExitCode : int
        {
            OK = 0,
            InvalidPassword = 1
        }

        private readonly MySqlConnection Connection = new MySqlConnection();

        public SQLCommands()
        {

        }

        private bool Connect(string user, string server, int port, string password)
        {
            try
            {
                CloseConnection();
                Connection.ConnectionString = $"server={server};user={user};database=mysql;port={port};password={password}";
                Connection.Open();

                return true;
            }

            catch (Exception ex)
            {
                CloseConnection();
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private void CloseConnection()
        {
            try
            {
                Connection.Close();
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private int ExecCmd(string cmd)
        {
            MySqlCommand myCmd = new MySqlCommand(cmd, Connection);

            int rowsAffected = myCmd.ExecuteNonQuery();
            myCmd.Dispose();

            return rowsAffected;
        }

        /// <summary>
        /// Creates a new MySQL user
        /// </summary>
        /// <param name="user">User name to create</param>
        /// <param name="server">MySQL server name to connect to</param>
        /// <param name="port">MySQL port to connect to</param>
        /// <param name="rootPass">root user's password</param>
        /// <param name="newPass">Password for the new user</param>
        /// <returns>true if successful; false if not</returns>
        public bool CreateNewUser(string user, string server, int port, string rootPass, string newPass)
        {
            bool success;

            try
            {
                bool connected = Connect("root", server, port, rootPass);

                if (!connected)
                {
                    CloseConnection();
                    return false;
                }

                ExecCmd($"create user if not exists '{user}'@'{server}' identified with mysql_native_password by '{newPass}'; flush privileges;");
                Console.WriteLine($"User '{user}' created sucessfully");

                success = true;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                success = false;
            }

            CloseConnection();

            return success;
        }

        /// <summary>
        /// Sets the password for a MySQL user
        /// </summary>
        /// <param name="user">The user to set password for</param>
        /// <param name="server">The server name to connect to</param>
        /// <param name="port">The port number to use</param>
        /// <param name="rootPass">Current password</param>
        /// <param name="newPass">New password</param>
        /// <returns>
        /// true if successful, false if not
        /// </returns>
        public bool SetUserPassword(string user, string server, int port, string rootPass, string newPass)
        {
            bool success;

            try
            {
                bool connected = Connect("root", server, port, rootPass);

                if (!connected)
                {
                    CloseConnection();
                    return false;
                }

                string pass_clean = MySqlHelper.EscapeString(newPass);

                ExecCmd($"alter user '{user}'@'{server}' identified with mysql_native_password by '{pass_clean}'; flush privileges;");

                //Console.WriteLine($"Password set to '{pass}', {rows} rows affected");
                Console.WriteLine($"Password set sucessfully for user '{user}'");

                success = true;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                success = false;
            }

            CloseConnection();

            return success;
        }

        /// <summary>
        /// Check if a given database exists by name
        /// </summary>
        /// <param name="server">The server name to connect to</param>
        /// <param name="port">The port number to use</param>
        /// <param name="rootPass">User's password</param>
        /// <param name="name">Name of database to check for</param>
        /// <returns>
        /// true if database exists, false if not, and null if there was an error checking it
        /// </returns>
        public bool? DatabaseExists(string server, int port, string rootPass, string name)
        {
            bool? exists;

            try
            {
                bool connected = Connect("root", server, port, rootPass);

                if (!connected)
                {
                    CloseConnection();
                    return null;
                }

                string sql = $"select count(schema_name) from information_schema.SCHEMATA where schema_name like '{name}';";
                //string sql = $"select schema_name from information_schema.SCHEMATA where schema_name like '{databaseName}';";

                MySqlCommand cmd = new MySqlCommand(sql, Connection);
                cmd.Parameters.Add(name, MySqlDbType.VarChar).Value = name;

                int numLikeName = Convert.ToInt32(cmd.ExecuteScalar());

                cmd.Dispose();

                exists = numLikeName > 0;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                exists = null;
            }

            CloseConnection();

            return exists;
        }

        /// <summary>
        /// Creates a database by name
        /// </summary>
        /// <param name="server">The server name to connect to</param>
        /// <param name="port">The port number to use</param>
        /// <param name="rootPass">User's password</param>
        /// <param name="name">Name of database to create</param>
        /// <returns>
        /// true if database creation successful, and false if not
        /// </returns>
        public bool CreateDatabase(string server, int port, string rootPass, string name)
        {
            bool success;

            try
            {
                bool connected = Connect("root", server, port, rootPass);

                if (!connected)
                {
                    CloseConnection();
                    return false;
                }

                ExecCmd($"create database if not exists `{name}`;");
                return true;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                success = false;
            }

            CloseConnection();

            return success;
        }

        /// <summary>
        /// Creates a database by name if it doesn't already exist
        /// </summary>
        /// <param name="server">The server name to connect to</param>
        /// <param name="port">The port number to use</param>
        /// <param name="rootPass">User's password</param>
        /// <param name="dbName">Name of database to create</param>
        /// <returns>
        /// true if database creation successful, and false if not
        /// </returns>
        public bool CreateDatabaseIfNotExists(string server, int port, string rootPass, string dbName)
        {
            if (String.IsNullOrWhiteSpace(dbName))
                return false;

            bool? exists = DatabaseExists(server, port, rootPass, dbName);

            if (exists != null && exists == false)
                return CreateDatabase(server, port, rootPass, dbName);

            return false;
        }

        public bool SetUserGrantsToDatabase(string user, string server, int port, string rootPass, string dbName)
        {
            bool success;

            try
            {
                bool connected = Connect("root", server, port, rootPass);

                if (!connected)
                {
                    CloseConnection();
                    return false;
                }

                ExecCmd($"grant all on `{dbName}`.* to '{user}'@'{server}';");
                return true;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                success = false;
            }

            CloseConnection();

            return success;
        }
    }
}

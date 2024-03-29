﻿#region License

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
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortableMySQL8
{
    public static class SQLTools
    {
        /// <summary>
        /// Check for the --initialize string in a given MySQL start parameter string
        /// </summary>
        /// <param name="prams">MySQL start parameters</param>
        /// <returns>
        /// true if MySQL needs to be initialized, false if not
        /// </returns>
        public static bool NeedsInit(string prams)
        {
            return prams.Contains("--initialize");
        }

        /// <summary>
        /// Get the MySQL start paramters given a my.ini path and MySQL data path
        /// </summary>
        /// <param name="myIni">Path to my.ini (MUST be full path)</param>
        /// <param name="myDataPath">Path to MySQL data directory</param>
        /// <returns>
        /// MySQL start parameters string
        /// </returns>
        public static string GetStartParams(string myIni, string myDataPath)
        {
            string prams = "--defaults-file=" + "\"" + myIni.FixDirSeperators() + "\" --standalone --explicit_defaults_for_timestamp";

            //No MySQL data directory found, let's initialize it.
            //Doing an insecure initialization because we will set
            //a password for it immediately after.
            if (!Directory.Exists(myDataPath))
                prams += " --initialize-insecure";

            return prams;
        }

        /// <summary>
        /// Creates a new my.ini at the path specified
        /// </summary>
        /// <param name="myIni">Path to new my.ini</param>
        /// <param name="port">Port MySQL will listen on</param>
        /// <param name="myBase">Base directory for MySQL install (MUST be full path)</param>
        /// <param name="myData">Data directory for MySQL database (MUST be full path)</param>
        /// <returns></returns>
        public static bool CreateNewMyIni(string myIni, int port, string myBase, string myData)
        {
            try
            {
                //Set up default my.ini file contents
                //I'm sure there's probably a better way to do this but this is simple enough for now.
                List<string> contents = new List<string>()
                {
                    "[client]",
                    "",
                    $"port={port}",
                    "",
                    "# The character set MySQL client will use",
                    "# MySQL defaults to utf8mb4 but OpenSim needs utf8mb3",
                    "default-character-set=utf8mb3",
                    "",
                    "[mysqld]",
                    "",
                    "# The TCP/IP Port the MySQL Server will listen on",
                    $"port={port}",
                    "",
                    "# The character set MySQL client will use",
                    "# MySQL defaults to utf8mb4 but OpenSim needs utf8mb3",
                    "character-set-server=utf8mb3",
                    "",
                    "#Path to installation directory. All paths are usually resolved relative to this.",
                    "basedir=" + "\"" + myBase.FixDirSeperators() + "\"",
                    "",
                    "#Path to the database root",
                    "datadir=" + "\"" + myData.FixDirSeperators() + "\"",
                    "",
                    "#OpenSim needs this on MySQL 8.0.4+",
                    "default-authentication-plugin=mysql_native_password",
                    "",
                    "#Max packetlength to send/receive from to server.",
                    "#MySQL's default seems to be 1, 4, or 16 MB depending on version, but OpenSim needs more than that",
                    "max_allowed_packet=128M"
                };

                //Dump the new my.ini file to the proper location
                File.WriteAllLines(myIni, contents);

                return true;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Updates my.ini configuration with values critical to running MySQL
        /// </summary>
        /// <param name="myIni">Path to my.ini config to update</param>
        /// <param name="port">Port to run MySQL on</param>
        /// <param name="myBase">Base directory for MySQL install (MUST be full path)</param>
        /// <param name="myData">Data directory for MySQL database (MUST be full path)</param>
        /// <returns>
        /// true if successful, false if not
        /// </returns>
        public static bool UpdateMyIni(string myIni, int port, string myBase, string myData)
        {
            try
            {
                Console.WriteLine($"Writing port config to {myIni}...");
                IniFile.WriteValue("client", "port", port.ToString(), myIni);
                IniFile.WriteValue("mysqld", "port", port.ToString(), myIni);

                Console.WriteLine($"Updating basedir and datadir in {myIni}...");
                IniFile.WriteValue("mysqld", "basedir", "\"" + myBase.FixDirSeperators() + "\"", myIni);
                IniFile.WriteValue("mysqld", "datadir", "\"" + myData.FixDirSeperators() + "\"", myIni);

                return true;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
    }
}

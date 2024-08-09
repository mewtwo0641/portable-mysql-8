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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace PortableMySQL8
{
    /// <summary>
    /// Interaction logic for TabDatabase.xaml
    /// </summary>
    public partial class TabDatabase : UserControl
    {
        private readonly MainWindow Instance = null;
        private readonly Settings Config = null;
        private readonly SQLCommands SQL = null;

        public TabDatabase(MainWindow _instance, Settings _config, SQLCommands _sql)
        {
            InitializeComponent();

            Instance = _instance;
            Config = _config;
            SQL = _sql;

            LoadUIConfig();
        }

        #region Events

        #region General

        private void TextBoxValidation_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !AreCharactersValid(e.Text);
        }

        #endregion General

        #region Login Info

        private void TextBoxDbUser_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConvertTextBoxSpacesToUnderscores(sender as TextBox);
            Config.Database.LoginUser = textBoxDbUser.Text.Trim();
        }

        private void TextBoxDbServer_TextChanged(object sender, TextChangedEventArgs e)
        {
            Config.Database.LoginServer = textBoxDbServer.Text.Trim();
        }

        private void PassWordBoxDbUserPass_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Config.Database.LoginPassword = passWordBoxDbUserPass.Password.Trim();
        }

        private void CheckBoxSaveLoginInfo_Click(object sender, RoutedEventArgs e)
        {
            if (checkBoxSaveLoginInfo.IsChecked == true)
            {
                MessageBoxResult result = MessageBox.Show($"Your database login info (user name, server name, and user password) will be stored in CLEAR TEXT in {Path.GetFileName(Instance.PathConfig)}!\r\n\r\nPlease note that these details have to be stored in clear text in OpenSim's various .ini config files anyway, so this may be a non-issue for some. If this is an issue for you, or you are unsure, click 'No'.\r\n\r\nAre you sure you want to do this?", "Confirm Allow Save Login Info", MessageBoxButton.YesNo);

                //User did anything but click "Yes"
                if (result != MessageBoxResult.Yes)
                    checkBoxSaveLoginInfo.IsChecked = false;
            }

            Config.Database.SaveLoginInfo = checkBoxSaveLoginInfo.IsChecked.TranslateNullableBool();
        }

        #endregion Login Info

        #region Database Details

        private void TextBoxMainName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConvertTextBoxSpacesToUnderscores(sender as TextBox);
            Config.Database.OSMain = textBoxMainName.Text.Trim();
        }

        private void TextBoxProfilesName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConvertTextBoxSpacesToUnderscores(sender as TextBox);
            Config.Database.OSProfiles = textBoxProfilesName.Text.Trim();
        }

        private void TextBoxGroupsName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConvertTextBoxSpacesToUnderscores(sender as TextBox);
            Config.Database.OSGroups = textBoxGroupsName.Text.Trim();
        }

        #endregion Database Details

        #region Buttons

        private void BtnCreateAll_Click(object sender, RoutedEventArgs e)
        {
            if (!ProcessHelpers.ProcessExists("mysqld"))
            {
                MessageBox.Show("Can't create user and/or database(s) because MySQL is not running", "Error");
                return;
            }

            if (String.IsNullOrWhiteSpace(Config.Database.LoginUser))
            {
                MessageBox.Show("The 'DB User Name' field is required!", "Error");
                return;
            }

            if (String.IsNullOrWhiteSpace(Config.Database.LoginPassword))
            {
                MessageBox.Show("The 'DB User Password' field is required!", "Error");
                return;
            }

            if (String.IsNullOrWhiteSpace(Config.Database.OSMain))
            {
                MessageBox.Show("The 'OpenSim Schema Name' field is required!", "Error");
                return;
            }

            string user = Config.Database.LoginUser;
            string server = "localhost";
            string password = Config.Database.LoginPassword;

            if (!string.IsNullOrWhiteSpace(Config.Database.LoginServer))
                server = Config.Database.LoginServer;

            #region Create User and Set Password

            //Create the user
            bool createdUser = SQL.CreateNewUser(user, server, Config.MySQL.Port, Config.MySQL.RootPass, password);

            if (!createdUser)
            {
                MessageBox.Show($"Was not able to create user '{user}'!", "Error");
                return;
            }

            //Set the password
            bool passwordSet = SQL.SetUserPassword(user, server, Config.MySQL.Port, Config.MySQL.RootPass, password);

            if (!passwordSet)
            {
                MessageBox.Show($"Was not able to set password for user '{user}'!", "Error");
                return;
            }

            #endregion Create User and Set Password

            #region Create Schemas

            string creationStatus = String.Empty;

            List<string> databases = new List<string>() { Config.Database.OSMain, Config.Database.OSProfiles, Config.Database.OSGroups };

            foreach (string db in databases)
            {
                if (String.IsNullOrWhiteSpace(db))
                    continue;

                bool success = SQL.CreateDatabaseIfNotExists(
                    server, Config.MySQL.Port, Config.MySQL.RootPass, db);

                if (!success)
                    Console.WriteLine($"Could not create database '{db}'");

                else
                {
                    bool grantsSet = SQL.SetUserGrantsToDatabase(user, server, Config.MySQL.Port, Config.MySQL.RootPass, db);

                    if (!grantsSet)
                        creationStatus += $"Set grants failed on '{db}'\r\n";
                }

                creationStatus += CreateStatusString(db, success) + "\r\n";
            }

            //Results
            MessageBox.Show(creationStatus, "Database Creation Results");

            #endregion Create Schemas
        }

        #endregion Buttons

        #endregion Events

        #region Methods

        private void LoadUIConfig()
        {
            textBoxDbUser.Text = Config.Database.LoginUser;
            textBoxDbServer.Text = Config.Database.LoginServer;
            passWordBoxDbUserPass.Password = Config.Database.LoginPassword;
            checkBoxSaveLoginInfo.IsChecked = Config.Database.SaveLoginInfo;

            textBoxMainName.Text = Config.Database.OSMain;
            textBoxProfilesName.Text = Config.Database.OSProfiles;
            textBoxGroupsName.Text = Config.Database.OSGroups;
        }

        private string CreateStatusString(string name, bool success)
        {
            string ret = String.Empty;

            ret += $"'{name}'";

            if (success)
                ret += ": Successfully created!";

            else
                ret += ": Creation failed! (Perhaps it already exists?)";

            return ret;
        }

        /// <summary>
        /// Validate string to contain only upper and lower case alphanumeric characters and underscores
        /// </summary>
        /// <param name="str">String to validate</param>
        /// <returns>true if valid; false if not</returns>
        private bool AreCharactersValid(string str)
        {
            Regex regex = new Regex("[a-zA-Z0-9_]", RegexOptions.IgnoreCase);

            foreach (char c in str)
            {
                if (!regex.IsMatch(c.ToString()))
                    return false;
            }

            return true;
        }

        private void ConvertTextBoxSpacesToUnderscores(TextBox tb)
        {
            try
            {
                int pos = tb.CaretIndex;

                if (tb.Text.Contains(" "))
                    tb.Text = tb.Text.Replace(" ", "_");

                tb.CaretIndex = pos;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        #endregion Methods
    }
}

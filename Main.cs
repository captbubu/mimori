
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Configuration;
using System.Net;
//using System.Net.Mail;
//using System.Net.Security; 

namespace mimori
{
    class Mail
    {
        public List<string> toList = new List<string>();
        List<string> ccList = new List<string>();
        List<string> bccList = new List<string>();
        public string subject { get; set; }
        public string message { get; set; }
    }
    public class User
    {
        public int port { get; set; }
        public string name { get; set; }
        public string password { get; set; }
        public string server { get; set; }

        public User(string server, string name, string password, int port)
        {
            this.name = name;
            this.password = password;
            this.server = server;
            this.port = port;
        }
    }
    class Mimori
    {
        public static User user { get; set; }

        public class MyCredentials : ICredentialsByHost
        {
            public NetworkCredential GetCredential(string host, int port, string authentication)
            {
                return new NetworkCredential(user.name, user.password);
            }
        }

        static void Main()
        {
            var config = ConfigurationManager.AppSettings;
            var user = new User(
                ReadSetting("user1.server"), ReadSetting("user1.name"), ReadSetting("User1.password"), int.Parse(ReadSetting("user1.port")));
                        
            Application.Run(new MainWindow());
        }

        static string ReadSetting(string key)
        {
            try
            {
                var appSetting = ConfigurationManager.AppSettings;
                string result = appSetting[key] ?? "N/A";
                return result;
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error loading config");
                return null;
            }
        }

        static void SaveUpdateConfig(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var setting = configFile.AppSettings.Settings;
                if (setting[key] == null)
                {
                    setting.Add(key, value);
                }
                else
                {
                    setting[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error saving settings.");
            }
        }
    }
}

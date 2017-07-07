
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Configuration;

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

        public User(string name, string server, int port)
        {
            this.name = name;
            this.server = server;
            this.port = port;
        }
    }
    class Mimori
    {
        static void Main()
        {
            var config = ConfigurationManager.AppSettings;
            //var user = new User(, , 587);
            saveUpdateConfig("user1.name", "miklos@magyari.hu");
            saveUpdateConfig("user1.server", "mail.magyari.hu");
            saveUpdateConfig("user1.port", "587");
            Application.Run(new MainWindow());
        }

        static void saveUpdateConfig(string key, string value)
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

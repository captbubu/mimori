
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace mimori
{
    class Mimori
    {
        public class Mail
        {
            public List<string> toList = new List<string>();
            List<string> ccList = new List<string>();
            List<string> bccList = new List<string>();
            public string subject { get; set; }
            public string message { get; set; }

            private static bool Myrms(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            {
                return true;
            }

            public void SendMessage()
            {
                var thread = new Thread(SendInBackground);
                thread.Start();
            }

            private void SendInBackground()
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(Myrms);
                var mail = new MailMessage(user.name, toList[0]);
                mail.Subject = subject;
                mail.Body = message;
                var smtpc = new SmtpClient(user.server, user.port);
                smtpc.UseDefaultCredentials = false;
                smtpc.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpc.Credentials = new MyCredentials().GetCredential(user.server, user.port, null);
                smtpc.EnableSsl = true;
                try
                {
                    smtpc.Send(mail);
                }
                catch (Exception)
                {

                }
            }
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

        public static User user;

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
            user = new User(
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

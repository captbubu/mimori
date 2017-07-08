
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Forms;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Net.Mime;

namespace mimori
{
    class Mimori
    {
        const string clientIdString = "Mimori 0.0.1";

        public class Mail
        {
            public List<string> toList = new List<string>();
            public List<string> ccList = new List<string>();
            public List<string> bccList = new List<string>();
            public string subject { get; set; }
            public string message { get; set; }
            public string[] headers;

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
                var headers = new NameValueCollection();
                headers.Add("X-Mailer", clientIdString);
                var mail = new MailMessage(user.name, toList.Count > 0 ? string.Join(",",  toList.ToArray()) : "");
                if (ccList.Count > 0)
                    mail.CC.Add(string.Join(",", ccList.ToArray()));
                if (bccList.Count > 0)
                    mail.Bcc.Add(string.Join(",", bccList.ToArray()));
                mail.Subject = subject;
                mail.Body = message;
                mail.Headers.Add(headers);
                var smtpc = new SmtpClient(user.smtpServer, user.smtpPort);
                smtpc.UseDefaultCredentials = false;
                smtpc.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpc.Credentials = new MyCredentials().GetCredential(user.smtpServer, user.smtpPort, null);
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
            public int smtpPort { get; set; }
            public int imapPort { get; set; }
            public string name { get; set; }
            public string password { get; set; }
            public string smtpServer { get; set; }
            public string imapServer { get; set; }

            public User(string smtpServer, int smtpPort, string imapServer, int imapPort, string name, string password)
            {
                this.name = name;
                this.password = password;
                this.smtpServer = smtpServer;
                this.smtpPort = smtpPort;
                this.imapServer = imapServer;
                this.imapPort = imapPort;
            }
        }

        public static IMAP imap;
        public static User user;
        public static MainWindow mw = new MainWindow();

        public class MyCredentials : ICredentialsByHost
        {
            public NetworkCredential GetCredential(string host, int port, string authentication)
            {
                return new NetworkCredential(user.name, user.password);
            }
        }

        public static void FetchImap()
        {
            imap.Auth(user.name, user.password);
            imap.SelectFolder("INBOX");
            imap.FetchHeaders(mw.dataGridView2);
        }

        static void Main()
        {
            var config = ConfigurationManager.AppSettings;
            user = new User(
                ReadSetting("user1.smtpserver"), int.Parse(ReadSetting("user1.smtpport")), ReadSetting("user1.imapserver"), int.Parse(ReadSetting("user1.imapport")),
                ReadSetting("user1.name"), ReadSetting("User1.password"));

            imap = new IMAP(user.imapServer, user.imapPort);
            Thread fimap = new Thread(FetchImap);
            fimap.Start();
            Application.Run(mw);
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

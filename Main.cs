
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace mimori
{
    class Mimori
    {
        const string clientIdString = "Mimori 0.0.2";

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

        public static MainWindow mw = new MainWindow();
        public static IMAP imap;
        public static User user;

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
            imap.FetchHeaders();
        }

        [STAThread]
        static void Main()
        {
            var config = ConfigurationManager.AppSettings;
            user = new User(
                ReadSetting("user1.smtpserver"), int.Parse(ReadSetting("user1.smtpport")), ReadSetting("user1.imapserver"), int.Parse(ReadSetting("user1.imapport")),
                ReadSetting("user1.name"), ReadSetting("User1.password"));

            imap = new IMAP(user.imapServer, user.imapPort);
            imap.LoadHeaders();
            Thread fimap = new Thread(FetchImap);
            fimap.Start();
            var refreshTimer = new System.Timers.Timer();
            refreshTimer.Interval = 3000;
            refreshTimer.Enabled = true;
            refreshTimer.Elapsed += new ElapsedEventHandler(refreshHappened);

            Application.Run(mw);
        }

        private static void refreshHappened(object source, ElapsedEventArgs args)
        {
            if (! mw.IsHandleCreated)
            {
                return;
            }

            if (mw.dataGridView2.RowCount == imap.NrUids)
            {
                return;
            }

            // Invoke() kell, különben thread-ek között adatmanipuláció miatt exception lesz.
            mw.dataGridView2.Invoke(new Action(() =>
            {
                // Kell egy másolat, mert ha az IMAP thread új elemet ad a foreach közben, exception lesz.
                var listCopy = new List<IMAP.MessageHeader>(imap.headers);
                //mw.dataGridView2.DataSource = listCopy.Select(o => new { Col_subject = o.Subject, Col_from = o.From, Col_date = o.Date } );
                
                foreach (IMAP.MessageHeader mh in listCopy)
                {
                    if (!imap.displayedHeaders.Contains(mh.UID))
                    {
                        DataGridViewRow row = (DataGridViewRow) mw.dataGridView2.Rows[0].Clone();
                        row.Cells[1].Value = mh.Subject;
                        row.Cells[2].Value = mh.From;
                        row.Cells[3].Value = mh.Date;
                        mw.dataGridView2.Rows.Add(row);
                        imap.displayedHeaders.Add(mh.UID);
                    }
                }    
            }));
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


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
using System.Net.Security;
using System.IO;

namespace mimori
{
    class Mimori
    {
        const string clientIdString = "Mimori 0.0.1";

        class IMAP
        {
            private static TcpClient tcpc = null;
            private static SslStream ssl = null;
            private static byte[] dummy;
            private static byte[] buffer;
            StringBuilder sb = new StringBuilder();
            public int prefix { get; set; }
            List<string> responses;

        public IMAP()
            {
                tcpc = new TcpClient(user.imapServer, user.imapPort);
                ssl = new SslStream(tcpc.GetStream(), false, Myrms, null);
                prefix = 1;
                responses = new List<string>();
            }
            private static bool Myrms(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            {
                return true;
            }
            private void Receive(string command)
            {
                try
                {
                    if (command != "")
                    {
                        if (tcpc.Connected)
                        {
                            dummy = Encoding.ASCII.GetBytes("X" + prefix.ToString() + " " + command);
                            ssl.Write(dummy, 0, dummy.Length);
                        }
                        else
                        {
                            Console.Write("tcp disconnected");
                        }
                    }
                    ssl.Flush();
                    buffer = new byte[2048];
                    do
                    {
                        int bytes = ssl.Read(buffer, 0, 2048);
                        sb.Append(Encoding.ASCII.GetString(buffer));
                        Array.Clear(buffer, 0, buffer.Length);
                    } while (! sb.ToString().Contains("X" + prefix.ToString() + " OK"));
                    responses.Add(sb.ToString());
                    prefix++;
                }
                catch (Exception)
                {
                    Console.WriteLine("IMAP command failed");
                }
            }
            public void Auth()
            {
                var stream = tcpc.GetStream();
                var writer = new StreamWriter(stream) { AutoFlush = true };
                var reader = new StreamReader(stream);
      
                var resp = reader.ReadLine();
                      
                writer.WriteLine("1 STARTTLS");
                resp = reader.ReadLine();
                
                ssl.AuthenticateAsClient(user.imapServer);
                string response;
                Receive("LOGIN " + user.name + " " + user.password + "\r\n");
                //Receive("LIST " + "\"\"" + " \"*\"" + "\r\n");
                Receive("SELECT INBOX\r\n");
                sb.Clear();
                Receive("UID FETCH 1:* FLAGS\r\n");
                response = sb.ToString();
                sb.Clear();
                Receive("UID FETCH 4969 (BODY.PEEK[HEADER])\r\n");
            }
        }

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
                ReadSetting("user1.smtpserver"), int.Parse(ReadSetting("user1.smtpport")), ReadSetting("user1.imapserver"), int.Parse(ReadSetting("user1.imapport")),
                ReadSetting("user1.name"), ReadSetting("User1.password"));

            var imap = new IMAP();
            imap.Auth();
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

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Net.Security;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Windows.Forms;

namespace mimori
{
    public class IMAP
    {
        public string server { get; set; }
        public int port { get; set; }
        private static TcpClient tcpc = null;
        private static SslStream ssl = null;
        private static byte[] dummy;
        private static byte[] buffer;
        StringBuilder sb = new StringBuilder();
        public int prefix { get; set; }
        List<string> responses;
        List<int> uidList = new List<int>();

        public IMAP(string server, int port)
        {
            this.server = server;
            this.port = port;
            tcpc = new TcpClient(server, port);
            ssl = new SslStream(tcpc.GetStream(), false, Myrms, null);
            prefix = 1;
            responses = new List<string>();
        }

        private static bool Myrms(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private string Receive(string command)
        {
            try
            {
                if (command != "")
                {
                    if (tcpc.Connected)
                    {
                        dummy = Encoding.ASCII.GetBytes("X" + prefix.ToString() + " " + command + "\r\n");
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
                } while (!sb.ToString().Contains("X" + prefix.ToString() + " OK"));
                string retval = sb.ToString();
                sb.Clear();
                prefix++;
                return retval;
            }
            catch (Exception)
            {
                Console.WriteLine("IMAP command failed");
                return null;
            }
        }

        public void Auth(string user, string password)
        {
            var stream = tcpc.GetStream();
            var writer = new StreamWriter(stream) { AutoFlush = true };
            var reader = new StreamReader(stream);

            var resp = reader.ReadLine();

            writer.WriteLine("1 STARTTLS");
            resp = reader.ReadLine();

            ssl.AuthenticateAsClient(server);
            Receive("LOGIN " + user + " " + password + "\r\n");
        }

        public void SelectFolder(string folder)
        {
            Receive("SELECT " + folder);
        }

        private void FetchUidList()
        {
            string response = Receive("UID FETCH 1:* FLAGS");
            using (StringReader sr = new StringReader(response))
            {
                string line;
                var resplist = new List<string>();
                while ((line = sr.ReadLine()) != null)
                {
                    string pattern = "UID\\s+(\\d+)";
                    Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
                    MatchCollection m = r.Matches(line);
                    foreach (Match match in m)
                    {
                        uidList.Add(int.Parse(match.Groups[1].Value));
                    }
                }
                responses = resplist;
            }
        }

        public void FetchHeaders(DataGridView dgv)
        {
            FetchUidList();
            int zz = 0;
            foreach (int msgIndex in uidList)
            {
                if (++zz == 100)
                    break;
                string response = Receive("UID FETCH " + msgIndex + " (FLAGS BODY[HEADER.FIELDS (DATE FROM SUBJECT)])");
                using (StringReader sr = new StringReader(response))
                {
                    string line;
                    var resplist = new List<string>();
                    while ((line = sr.ReadLine()) != null) {
                        string pattern = "^From:\\s+(.*)$";
                        Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
                        MatchCollection m = r.Matches(line);
                        foreach (Match match in m)
                        {
                            byte[] lofasz;
                            lofasz = Encoding.UTF8.GetBytes(match.Groups[1].Value);
                            string from = Encoding.Default.GetString(lofasz);
                            DataGridViewRow row = (DataGridViewRow) dgv.Rows[0].Clone();
                            row.Cells[0].Value = from;
                            dgv.Rows.Add(row);
                        }
                    }
                }
                response = sb.ToString();
            }
        }
    }
}

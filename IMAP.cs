using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Net.Security;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace mimori
{
    public class IMAP
    {
        public class MessageHeader
        {
            public MessageHeader(int uid, string from = null, string subject = null, string date = null)
            {
                UID = uid;
                Subject = subject;
                From = from;
                Date = date;
            }
            public int UID { get; }
            public string Subject { get; }
            public string From { get; }
            public string Date { get; }
        }

        public string server { get; set; }
        public int port { get; set; }
        private static TcpClient tcpc = null;
        private static SslStream ssl = null;
        public int NrUids { get; set; }
        public int prefix { get; set; }
        List<string> responses;
        List<int> uidList = new List<int>();
        public List<MessageHeader> headers = new List<MessageHeader>();
        public List<int> displayedHeaders = new List<int>();

        public IMAP(string server, int port)
        {
            this.server = server;
            this.port = port;
            tcpc = new TcpClient(server, port);
            ssl = new SslStream(tcpc.GetStream(), false, Myrms, null);
            prefix = 1;
            responses = new List<string>();
            NrUids = 0;
        }

        private static bool Myrms(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private string Receive(string command)
        {
            byte[] dummy;
            byte[] buffer;
            StringBuilder sb = new StringBuilder();

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
            NrUids = uidList.Count;
        }

        private void GetNamedValue(string line, string item, ref string store)
        {
            string pattern = item + @":\s+(.*)";
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
            Match match = r.Match(line);
            if (match.Success)
            {
                byte[] bytes = Encoding.Default.GetBytes(match.Groups[1].Value);
                store = DecodeString(Encoding.Default.GetString(bytes));
            }
        }

        public void FetchHeaders()
        {
            FetchUidList();

            string from;
            string subject;
            string date;

            int uid;
            foreach (int msgIndex in uidList)
            {
                from = null;
                subject = null;
                date = null;
                string response = Receive("UID FETCH " + msgIndex + " (FLAGS BODY[HEADER.FIELDS (DATE FROM SUBJECT)])");
                uid = msgIndex;
                using (StringReader sr = new StringReader(response))
                {
                    string line;
                    var resplist = new List<string>();
                    while ((line = sr.ReadLine()) != null) {
                        GetNamedValue(line, "from", ref from);
                        GetNamedValue(line, "subject", ref subject);
                        GetNamedValue(line, "date", ref date);
                    }
                }
                headers.Add(new MessageHeader(uid : uid, from: from, subject: subject, date : date));
            }
        }

        private string DecodeString(string instr)
        {
            string pattern = @"=\?([\w\d-]+)\?([QB])\?([\w\d_\-\.=+:,]+)\?=";
            Regex r = new Regex(pattern);
            MatchCollection m = r.Matches(instr);
            string readable = null;
            if (m.Count > 0)
            {
                foreach (Match match in m)
                {
                    string encoding = match.Groups[1].Value;
                    bool enctype = match.Groups[2].Value == "Q" ? true : false;
                    string toDecode = match.Groups[3].Value;

                    try
                    {
                        Encoding enc = Encoding.GetEncoding(encoding);
                        if (enctype == false)
                        {
                            // Base64
                            var bytes = Convert.FromBase64String(toDecode);
                            readable = enc.GetString(bytes);
                        }
                        else
                        {
                            // Quted printable
                            readable = DecodeQuotedPrintables(toDecode, encoding);
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }
            else
            {
                readable = instr;
            }
            return readable;
        }

        private static string DecodeQuotedPrintables(string input, string charSet)
        {
            if (string.IsNullOrEmpty(charSet))
            {
                var charSetOccurences = new Regex(@"=\?.*\?Q\?", RegexOptions.IgnoreCase);
                var charSetMatches = charSetOccurences.Matches(input);
                foreach (Match match in charSetMatches)
                {
                    charSet = match.Groups[0].Value.Replace("=?", "").Replace("?Q?", "");
                    input = input.Replace(match.Groups[0].Value, "").Replace("?=", "");
                }
            }

            Encoding enc = new ASCIIEncoding();
            if (!string.IsNullOrEmpty(charSet))
            {
                try
                {
                    enc = Encoding.GetEncoding(charSet);
                }
                catch
                {
                    enc = new ASCIIEncoding();
                }
            }

            //decode iso-8859-[0-9]
            var occurences = new Regex(@"=[0-9A-Z]{2}", RegexOptions.Multiline);
            var matches = occurences.Matches(input);
            foreach (Match match in matches)
            {
                try
                {
                    byte[] b = new byte[] { byte.Parse(match.Groups[0].Value.Substring(1), System.Globalization.NumberStyles.AllowHexSpecifier) };
                    char[] hexChar = enc.GetChars(b);
                    input = input.Replace(match.Groups[0].Value, hexChar[0].ToString());
                }
                catch
                {; }
            }

            //decode base64String (utf-8?B?)
            occurences = new Regex(@"\?utf-8\?B\?.*\?", RegexOptions.IgnoreCase);
            matches = occurences.Matches(input);
            foreach (Match match in matches)
            {
                byte[] b = Convert.FromBase64String(match.Groups[0].Value.Replace("?utf-8?B?", "").Replace("?UTF-8?B?", "").Replace("?", ""));
                string temp = Encoding.UTF8.GetString(b);
                input = input.Replace(match.Groups[0].Value, temp);
            }

            input = input.Replace("=\r\n", "");

            return input;
        }
    }
}

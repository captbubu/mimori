
using System.Collections.Generic;
using System.Windows.Forms;

namespace mimori
{
    class Mail
    {
        List<string> toList = new List<string>();
        List<string> ccList = new List<string>();
        List<string> bccList = new List<string>();
        string subject;
        string message;
    }
    class Mimori
    {
        static void Main()
        {
            Application.Run(new MainWindow());
        }
    }
}

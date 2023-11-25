using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lab1_dotnet_framework
{
    public partial class Help : Form
    {
        public Help()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string encodedurl = WebUtility.HtmlEncode(@"" + Directory.GetCurrentDirectory() + "/../../../help/help.html");
            webBrowser1.Navigate(encodedurl);
        }
    }
}

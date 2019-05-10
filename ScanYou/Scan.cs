using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScanYou
{
    public partial class Scan : Form
    {
        private bool stop = false;
        private const string ButtonStart = "Scan";
        private const string ButtonStop = "Stop";
        public Scan()
        {
            InitializeComponent();
            btnGo.Text = ButtonStart;
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            stop = btnGo.Text == ButtonStop;
            btnGo.Text = btnGo.Text == ButtonStart ? ButtonStop : ButtonStart;

            if (!stop)
            {
                listFound.Items.Clear();

                var upper1 = LowerUpper(ip1.Text, out var lower1);
                var upper2 = LowerUpper(ip2.Text, out var lower2);
                var upper3 = LowerUpper(ip3.Text, out var lower3);
                var upper4 = LowerUpper(ip4.Text, out var lower4);

                Task.Run(() =>
                {
                    for (int x = lower1; (x <= upper1 && !stop); x++)
                    {
                        for (int y = lower2; (y <= upper2 && !stop) ; y++)
                        {
                            for (int i = lower3; (i <= upper3 && !stop) ; i++)
                            {
                                for (int j = lower4; (j <= upper4 && !stop) ; j++)
                                {
                                    string ip = $"{x}.{y}.{i}.{j}";
                                    lblStatus.Invoke(new MethodInvoker(() =>
                                    {
                                        lblStatus.Text = $"Status: Checking {ip} : {port.Text}";
                                        lblLastCheck.Text = $"Last Checked: {ip} : {port.Text}";
                                        Application.DoEvents();
                                    }));

                                    if (IsPortOpen(ip, Convert.ToInt32(port.Text), new TimeSpan(0, 0, 0, 0, 5)))
                                        listFound.Invoke(new MethodInvoker(() =>
                                        {
                                            listFound.Items.Add($"{ip} : {port.Text}");
                                            Application.DoEvents();
                                        }));
                                }
                            }
                        }
                    }

                    lblStatus.Invoke(new MethodInvoker(() =>
                    {
                        lblStatus.Text = "Status: Idle";

                        stop = false;
                        btnGo.Text = ButtonStart;
                    }));
                });
            }
        }

        private int LowerUpper(string value, out int lower)
        {
            int upper = 255;
            if (int.TryParse(value, out lower))
                upper = lower;
            return upper;
        }

        private bool IsPortOpen(string host, int port, TimeSpan timeout)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect(host, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(timeout);
                    if (!success)
                    {
                        return false;
                    }

                    client.EndConnect(result);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        private void toolStripMenuItemExport_Click(object sender, EventArgs e)
        {
            string s = string.Empty;
            foreach (object o in listFound.SelectedItems)
            {
                s += o + Environment.NewLine;
            }

            if (s != string.Empty)
                Clipboard.SetText(s);
        }
    }
}

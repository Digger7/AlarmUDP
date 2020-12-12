using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using WindowsInput;
using System.Configuration;
using Microsoft.Win32;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        int PORT = 9876;
        UdpClient udpClient = new UdpClient();

        bool exitButton = false;

        int timerRunCount = 0;

        public Form1()
        {
            InitializeComponent();

            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, PORT));

            var from = new IPEndPoint(0, 0);
            Task.Run(() =>
            {
                while (true)
                {
                    var recvBuffer = udpClient.Receive(ref from);
                    string message = Encoding.UTF8.GetString(recvBuffer);

                    string[] items = message.Split(';');

                    string userName = "";
                    string mashineName = "";

                    if (items.Length == 2) {
                        userName = items[0];
                        mashineName = items[1];
                    }

                    if (userName != Environment.UserName && mashineName != Environment.MachineName)
                    {
                        if (!String.IsNullOrEmpty(userName) && !String.IsNullOrEmpty(mashineName)) {
                            //var fAlert = new FormAlert(userName, mashineName, DateTime.Now.ToLongTimeString());

                            var fAlert = new FormAlert(userName, mashineName,$"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}");

                            fAlert.StartPosition = FormStartPosition.CenterParent;
                            //fAlert.TopMost = true;
                            fAlert.ShowDialog(this);
                        }
                    }
                }
            });

        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon1.Visible = true;
            }  
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;  
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (exitButton == false) {
                Hide();
                notifyIcon1.Visible = true;
                e.Cancel = true;                        
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            exitButton = true;
            Application.Exit();
        }

        private void ВыходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Выйти из приложения?", "Вы уверены?",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                exitButton = true;
                Application.Exit();
            }
        }

        private async void ButtonSave_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Сохранить параметры?", "Вы уверены?",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                KeyValueConfigurationCollection confCollection = config.AppSettings.Settings;

                confCollection["autoStart"].Value = checkBoxAutostart.Checked.ToString();
                confCollection["minimize"].Value = checkBoxMinimize.Checked.ToString();

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            checkBoxAutostart.Checked = Convert.ToBoolean(ConfigurationManager.AppSettings["autoStart"]);
            checkBoxMinimize.Checked = Convert.ToBoolean(ConfigurationManager.AppSettings["minimize"]);
            exitButton = !checkBoxMinimize.Checked;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["minimize"])) {
                Hide();
                notifyIcon1.Visible = true;
            }
        }

        private void CheckBoxAutostart_CheckedChanged(object sender, EventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (checkBoxAutostart.Checked)
            {
                key.SetValue("WebDavObjectsSpy", Application.ExecutablePath);
            }
            else {
                key.DeleteValue("WebDavObjectsSpy", false);
            }
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            KeyValueConfigurationCollection confCollection = config.AppSettings.Settings;
            confCollection["autoStart"].Value = checkBoxAutostart.Checked.ToString();
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
        }

        private void CheckBoxMinimize_CheckedChanged(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            KeyValueConfigurationCollection confCollection = config.AppSettings.Settings;
            confCollection["minimize"].Value = checkBoxMinimize.Checked.ToString();
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
        }

        private void buttonAlert_Click(object sender, EventArgs e)
        {
            var data = Encoding.UTF8.GetBytes($"{Environment.UserName};{Environment.MachineName}");
            udpClient.Send(data, data.Length, ConfigurationManager.AppSettings["ip"], PORT);
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabelAlarm.Visible = !toolStripStatusLabelAlarm.Visible;
            timerRunCount++;
            if (timerRunCount >= 20) {
                timerRunCount = 0;
                timer1.Enabled = false;
            }
            
        }
    }
}

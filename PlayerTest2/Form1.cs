using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Configuration;

//TODO recheck online offline every so often
//TODO limit combobox


namespace PlayerTest2
{
    public partial class Form1 : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        bool textChanged = false;
        bool loading = false;
        bool online = false;
        public static List<FollowingStreams> streams = new List<FollowingStreams>();
        DateTime lastUpdated = DateTime.Now;


        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        public Form1()
        {
            InitializeComponent();
            LoadFollowers(ConfigurationManager.AppSettings["username"]);
            timer1.Start();
            timer1.Interval = 400000;
        }

        private void LoadFollowers(string username)
        {
            string json = "";
            if (username == "")
            {
                username = GetUsername();
            }
            using (WebClient wc = new WebClient())
            {
                json = wc.DownloadString("https://api.twitch.tv/kraken/users/" + username +"/follows/channels?limit=100");
            }
            dynamic followInfo = JObject.Parse(json);
            foreach (dynamic d in followInfo.follows)
            {
                string name = d.channel.name;
                string game = d.channel.game;
                bool isPartner = d.channel.partner;
                bool isOnline = false;
                streams.Add(new FollowingStreams(name, game, isOnline, isPartner));
            }
            streams = streams.OrderBy(o => o.UserName).ToList();
            CheckOnlineDropDown();
            streams.Add(new FollowingStreams("Other", "",  true, false));
            AddFollowersToDropDown();
        }

        private void AddFollowersToDropDown()
        {
           onlineSelector1.Items.Clear();
            for (int i = 0; i<streams.Count; i++)
            {
               onlineSelector1.Items.Add(streams[i].UserName);
            }
        }

        private string GetUsername()
        {
            GetTwitchUsername form = new GetTwitchUsername();
            form.ShowDialog();
            return ConfigurationManager.AppSettings["username"];
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string quality = "";
            string path = "";
            string command = "";
            if (onlineSelector1.Text == "Other")
            {
                if (!online)
                    return;
                if (comboBox1.Text == "")
                    return;
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
                quality = comboBox1.Text;
                path = @"C:\\Program Files (x86)\\Livestreamer\\livestreamer.exe";
                command = @"twitch.tv/" + textBox1.Text + " " + quality;
            }
            else
            {
                FollowingStreams f = getStreamByName(onlineSelector1.Text);
                if (!f.Online)
                    return;
                if (comboBox1.Text == "")
                    return;
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
                quality = comboBox1.Text;
                path = @"C:\\Program Files (x86)\\Livestreamer\\livestreamer.exe";
                command = @"twitch.tv/" + f.UserName + " " + quality;
            }

            try
            {
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        Arguments = command,
                        FileName = path,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        WorkingDirectory = "C:\\Program Files (x86)\\Livestreamer"
                    }
                };
                proc.Start();
                string error = proc.StandardError.ReadToEnd();
                this.ShowInTaskbar = true;
                this.WindowState = FormWindowState.Normal;
                if (error != string.Empty)
                {
                    MessageBox.Show(error);
                }
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
            }
            catch (Exception)
            {

            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            timer1.Interval = 500;
            lastUpdated = DateTime.Now;
            if (!loading)
            {
                loading = true;
                online = false;
                pictureBox1.Image = Properties.Resources.loading_gif;
            }
            textChanged = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            TimeSpan ts = DateTime.Now - lastUpdated;

            if (ts.TotalMilliseconds > 1500 && textChanged)
            {
                checkIfOnline();
                textChanged = false;
            }
        }

        private void checkIfOnline()
        {
            try
            {
                if (textBox1.Text == "")
                {
                    pictureBox1.Image = Properties.Resources._2000px_Disc_Plain_red_svg;
                    comboBox1.Items.Clear();
                    return;
                }
                string json = "";
                loading = false;
                using (WebClient wc = new WebClient())
                {
                    json = wc.DownloadString("https://api.twitch.tv/kraken/streams/" + textBox1.Text);
                }
                dynamic streamInfo = JObject.Parse(json);
                if (streamInfo.stream == null)
                {
                    pictureBox1.Image = Properties.Resources._2000px_Disc_Plain_red_svg;
                    online = false;
                }
                else
                {
                    pictureBox1.Image = Properties.Resources._600px_Ski_trail_rating_symbol_green_circle_svg;
                    online = true;
                }
                if (online)
                {
                    if ((bool)streamInfo.stream.channel.partner)
                    {
                        comboBox1.Items.Clear();
                        comboBox1.Items.AddRange(new string[] { "Source", "High", "Medium", "Low", "Audio" });
                        comboBox1.SelectedItem = comboBox1.Items[0];
                    }
                    else 
                    {
                        comboBox1.Items.Clear();
                        comboBox1.Items.AddRange(new string[] { "Source", "Audio" });
                        comboBox1.SelectedItem = comboBox1.Items[0];
                    }  
                }
            }
            catch (Exception)
            {
                comboBox1.Items.Clear();
                pictureBox1.Image = Properties.Resources._2000px_Disc_Plain_red_svg;
                online = false;
            }
        }

        private void makeMoveable(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public static FollowingStreams GetStreamByName(string p)
        {
            foreach (FollowingStreams s in streams)
            {
                if (s.UserName == p)
                    return s;
            }
            return null;
        }

        private void CheckOnlineDropDown()
        {
            string follows = "";
            string json = "";
            foreach (FollowingStreams f in streams)
            {
                follows += f.UserName;
                follows += ",";
            }
            follows = follows.TrimEnd(',');

            using (WebClient wc = new WebClient())
            {
                json = wc.DownloadString("https://api.twitch.tv/kraken/streams?channel=" + follows);
            }
            dynamic onlineUsers = JObject.Parse(json);

            foreach (dynamic d in onlineUsers.streams)
            {
                for (int i = 0; i < streams.Count; i++)
                {
                    if (d.channel.name == streams[i].UserName)
                    {
                        streams[i].Online = true;
                    }
                }
            }
        }

        private void onlineSelector1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            if (onlineSelector1.Text == "Other")
            {
                textBox1.Visible = true;
            }
            else
            {
                textBox1.Visible = false;
            }
            if (getStreamByName(onlineSelector1.Text).Partner)
            {
                comboBox1.Items.Clear();
                comboBox1.Items.AddRange(new string[] { "Source", "High", "Medium", "Low", "Audio" });
                comboBox1.SelectedItem = comboBox1.Items[0];
            }
            else if (onlineSelector1.Text != "Other")
            {
                comboBox1.Items.Clear();
                comboBox1.Items.AddRange(new string[] { "Source", "Audio" });
                comboBox1.SelectedItem = comboBox1.Items[0];
            }
            else
            {
                comboBox1.Items.Clear();
            }   
        }
        private FollowingStreams getStreamByName(string name)
        {
            foreach (FollowingStreams f in streams)
            {
                if (name == f.UserName)
                    return f;
            }
            return null;
        }
    }
}
